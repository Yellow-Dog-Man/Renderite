using CSCore;
using EnumsNET;
using Renderite.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace Renderite.Unity
{
    class SilenceSource : CSCore.IWaveSource
    {
        public bool CanSeek => false;
        public WaveFormat WaveFormat { get; private set; }

        public long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public long Length => throw new NotSupportedException();

        public void Dispose()
        {
            
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            Array.Clear(buffer, offset, count);
            return count;
        }

        public SilenceSource(WaveFormat format)
        {
            this.WaveFormat = format;
        }
    }

    public class RenderingManager : MonoBehaviour
    {
        public const float MAX_PROCESSING_MS = 2;
        public const float MAX_PARTICLE_PROCESSING_MS = 4;

        public UnityEngine.Shader NullShader { get; private set; }
        public UnityEngine.Shader InvisibleShader { get; private set; }
        public UnityEngine.Material NullMaterial { get; private set; }
        public UnityEngine.Material InvisibleMaterial { get; private set; }

        public static bool IsDebug { get; private set; }

        public bool DebugFramePacing;

        public static RenderingManager Instance { get; private set; }

        public System.Diagnostics.Process? MainProcess { get; private set; }
        bool HasMainProcessExited
        {
            get
            {
                if (Renderite.Shared.Helper.IsWine || MainProcess is null)
                    return !Directory.Exists($"/proc/{_mainProcessId}");
                else
                    return MainProcess.HasExited;
            }
        }

        public string EditorQueueName;
        public long EditorQueueCapacity;

        public EngineInitProgress Progress;
        public Camera OverlayCamera;
        public CameraInitializer CameraInitializer;
        public KeyboardInput Keyboard;
        public MouseInput Mouse;
        public WindowInput Window;
        public DisplayInput Display;
        public List<InputDriver> InputDrivers;

        public VideoPlaybackManager VideoPlaybackManager;

        public HeadOutput VROutput => _vrOutput;
        public HeadOutput ScreenOutput => _screenOutput;

        public bool RendererDecoupled => !_lockStepActivated || _decoupleActive;

        public int LastFrameIndex { get; private set; } = -1;

        public float NearClip { get; private set; } = 0.01f;
        public float FarClip { get; private set; } = 1024f;
        public float DesktopFOV { get; private set; } = 75f;

        public SharedMemoryAccessor SharedMemory { get; private set; }
        public AssetIntegrator AssetIntegrator { get; private set; }
        public PerformanceStats Stats { get; private set; }

        public FrameResultsManager Results { get; set; }

        public AssetManager<MeshAsset> Meshes { get; private set; }
        public AssetManager<ShaderAsset> Shaders { get; private set; }
        public AssetManager<Texture2DAsset> Texture2Ds { get; private set; }
        public AssetManager<Texture3DAsset> Texture3Ds { get; private set; }
        public AssetManager<CubemapAsset> Cubemaps { get; private set; }
        public AssetManager<RenderTextureAsset> RenderTextures { get; private set; }
        public AssetManager<VideoTextureAsset> VideoTextures { get; private set; }
        public AssetManager<DesktopTextureAsset> DesktopTextures { get; private set; }
        public MaterialAssetManager Materials { get; private set; }
        public AssetManager<PointRenderBufferAsset> PointRenderBuffers { get; private set; }
        public AssetManager<TrailsRenderBufferAsset> TrailsRenderBuffers { get; private set; }
        public AssetManager<GaussianSplatAsset> GaussianSplats { get; private set; }

        public Action<PostProcessingConfig> PostProcessingUpdated;

        public InputManager Input { get; private set; }

        public Guid UniqueSessionId { get; private set; }

        FrameSubmitData _dataWithRenderTasks;

        Dictionary<int, RenderSpace> _renderSpaces = new Dictionary<int, RenderSpace>();
        Dictionary<int, LightsBufferRenderer> _lightBuffers = new Dictionary<int, LightsBufferRenderer>();

        List<int> _spacesToRemove = new List<int>();

        MessagingManager _primaryMessagingManager;
        MessagingManager _backgroundMessagingManager;

        ManualResetEventSlim _processingReady;
        volatile FrameSubmitData _frameData;

        PostProcessingConfig _postProcessing;
        QualityConfig _quality;
        ResolutionConfig _resolution;

        FrameStartData _frameStart;

        int? _maxBackgroundFPS;
        int? _maxForegroundFPS;
        bool _useVSync;

        bool _initReceived;
        RendererInitData _initData;
        int _mainProcessId;

        bool _initFinalized;

        volatile bool _fatalError;

        HeadOutput _vrOutput;
        HeadOutput _screenOutput;

        bool _shutdown;

        bool? _lastVRactive;

        // IMPORTANT!!! We start this as true, because we need the renderer to "kick-off" the frame processing by sending
        // frame begin, otherwise it would never start. Even though technically it didn't process a frame before.
        // We just don't want it to send new frame start until it actually gets a frame from the main process
        bool _lastFrameDataProcessed = true;

        bool _lockStepActivated;

        bool _decoupleActive;
        int _recoupleFrames;

        float _decoupleActivationInterval = 1f / 15f; // By default maintain at least 15 FPS
        int _recoupleFrameCount = 10;
        float _decoupledMaxAssetProcessingTime = 2f * 0.001f; // 4 ms

        System.Diagnostics.Stopwatch processingStopwatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch readyToFrameStopwatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch processedFrameToNextBegin = new System.Diagnostics.Stopwatch();

        void Awake()
        {
            if (Instance != null)
                throw new InvalidOperationException("Only one RenderingManager can exist");

            IsDebug = Application.isEditor;

            Instance = this;

            // Initialize connection to the main process
            if (!GetConnectionParameters(out var queueName, out var queueCapacity))
            {
                Debug.LogWarning("Could not get queue parameters to connect to");
                Application.Quit(1);
                return;
            }

            // Got the port, try connecting
            try
            {
                Debug.Log($"Connecting to queue {queueName} (capacity: {queueCapacity}");

                _primaryMessagingManager = new MessagingManager(PackerMemoryPool.Instance);

                _primaryMessagingManager.CommandHandler = HandleRenderCommand;
                _primaryMessagingManager.FailureHandler = HandleMessagingFailure;
                _primaryMessagingManager.WarningHandler = str => Debug.LogWarning(str);

                // Connect to the port to start receiving commands
                _primaryMessagingManager.Connect(queueName + Renderite.Shared.Helper.PRIMARY_QUEUE, false, queueCapacity);

                _backgroundMessagingManager = new MessagingManager(PackerMemoryPool.Instance);

                _backgroundMessagingManager.CommandHandler = HandleRenderCommand;
                _backgroundMessagingManager.FailureHandler = HandleMessagingFailure;
                _backgroundMessagingManager.WarningHandler = str => Debug.LogWarning(str);

                // Connect to the port to start receiving commands
                _backgroundMessagingManager.Connect(queueName + Renderite.Shared.Helper.BACKGROUND_QUEUE, false, queueCapacity);

                // Basic setup
                Application.targetFrameRate = -1;
                Application.wantsToQuit += OnAppWantsToQuit;
                QualitySettings.vSyncCount = 0;

                NullShader = UnityEngine.Shader.Find("BuiltIn/Null");
                InvisibleShader = UnityEngine.Shader.Find("BuiltIn/Invisible");
                NullMaterial = new UnityEngine.Material(NullShader);
                InvisibleMaterial = new UnityEngine.Material(InvisibleShader);

                // Initialize the camera renderer
                CameraRenderer.Initialize();

                Debug.Log("Connected to queue");

                StartCoroutine(RenderTaskProcessor());
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to connect to the controller:\n" + ex);
                Application.Quit(2);
                return;
            }
        }

        FrameStartData BeginFrame()
        {
            if (_frameStart == null)
                _frameStart = new FrameStartData();

            _frameStart.lastFrameIndex = LastFrameIndex;

            Stats.UpdateStats(_frameStart);

            Results.CollectResults(_frameStart);

            // Collect input state. The instance is shared every frame, so we don't have to constantly keep
            // allocating new instances. Instead we share the instance and just re-serialize it when it's updated
            Input.UpdateState();
            _frameStart.inputs = Input.State;

            return _frameStart;
        }

        void Update()
        {
            if (_shutdown)
            {
                Debug.Log("Shutting down");

                Application.Quit();
                return;
            }

            try
            {
                HandleUpdate();
            }
            catch (Exception ex)
            {
                _fatalError = true;

                Debug.LogError($"FATAL Exception handling update!\n{ex}");

                // This is fatal, we can't continue execution
                Application.Quit(4);

                return;
            }
        }

        void RunAssetIntegration()
        {
            if(_lastFrameDataProcessed)
            {
                // Reset all stats that can accumulate over multiple engine frames
                Stats.RenderedFramesSinceLast = 0;

                Stats.IntegrationProcessingTime = 0;
                Stats.ExtraParticleProcessingTime = 0;

                Stats.ProcessedAssetIntegratorTasks = 0;
                Stats.ProcessingHandleWaits = 0;
                Stats.IntegrationHighPriorityTasks = 0;
                Stats.IntegrationTasks = 0;
                Stats.IntegrationRenderTasks = 0;
                Stats.IntegrationParticleTasks = 0;
            }

            processingStopwatch.Restart();

            int processedAssetIntegratorTaks = 0;
            int handleWaits = 0;

            // Process the current batch of the delayed removals
            AssetIntegrator.ProcessDelayedRemovals();

            // We explicitly run these just once per update and at the beginning
            AssetIntegrator.RunRenderThreadUploads(MAX_PROCESSING_MS);

            // Allow at least one execution of the asset integration
            // TODO!!! Just rework this to a do-while loop?
            bool firstIteration = true;

            // Wait for the frame submission
            while (_frameData == null)
            {
                var elapsedTime = processingStopwatch.Elapsed.TotalSeconds;

                // If the renderer is currently de-coupled, only process
                if (RendererDecoupled && elapsedTime >= _decoupledMaxAssetProcessingTime && !firstIteration)
                    break;

                // If(Engine.IsLagging)
                if(!RendererDecoupled && elapsedTime >= _decoupleActivationInterval)
                {
                    // Don't()

                    // Activate de-coupling!
                    _decoupleActive = true;
                    _recoupleFrames = 0;
                }

                firstIteration = false;

                // Process any queues. If we processed anything, don't bother sleeping yet, just loop around
                // again, to check if we got frame data already and process more stuff if not.
                if (AssetIntegrator.Process())
                {
                    processedAssetIntegratorTaks++;
                    continue;
                }

                // If the render is de-coupled and there's nothing to process, just break the loop and render a frame
                if (RendererDecoupled)
                    break;

                // There's nothing to process now, so sleep until something comes in
                // That can be either a frame ready to be rendered or more asset integration
                handleWaits++;

                // Check again so we avoid waiting on handle unnecessarily
                if (_frameData == null)
                {
                    // Decouple activation interval can potentially be really large value, don't wait more than 1 second
                    // It's really unlikely that we'd ever need to wait longer than that - even if decoupling is not activate
                    // all this will do is just run the loop once a second, which is okay
                    if(_processingReady.Wait(Mathf.CeilToInt(Mathf.Clamp(MathHelper.FilterInvalid(_decoupleActivationInterval) * 1000, 0, 1000))))
                        _processingReady.Reset();
                }

                if (_fatalError)
                    return;
            }

            processingStopwatch.Stop();
            Stats.IntegrationProcessingTime += (float)processingStopwatch.Elapsed.TotalSeconds;

            processingStopwatch.Restart();

            while (AssetIntegrator.ProcessParticleQueueTask())
            {
                var remainingTime = MAX_PARTICLE_PROCESSING_MS - processingStopwatch.Elapsed.TotalMilliseconds;

                if (remainingTime <= 0)
                    break;
            }

            processingStopwatch.Stop();

            Stats.RenderedFramesSinceLast++;

            Stats.ExtraParticleProcessingTime += (float)processingStopwatch.Elapsed.TotalSeconds;

            Stats.ProcessedAssetIntegratorTasks += processedAssetIntegratorTaks;
            Stats.ProcessingHandleWaits += handleWaits;
            Stats.IntegrationHighPriorityTasks += AssetIntegrator.HighPriorityTasks;
            Stats.IntegrationTasks += AssetIntegrator.NormalTasks;
            Stats.IntegrationRenderTasks += AssetIntegrator.RenderThreadTasks;
            Stats.IntegrationParticleTasks += AssetIntegrator.ParticleTasks;

            Stats.FrameBeginToSubmitTime = (float)readyToFrameStopwatch.Elapsed.TotalSeconds;
            Stats.FrameProcessedToNextBeginTime = (float)processedFrameToNextBegin.Elapsed.TotalSeconds;

            // Check if we should disable decoupling
            if (_frameData != null && _decoupleActive)
            {
                var timeSinceLastFrameSeconds = Stats.FrameBeginToSubmitTime;

                if (timeSinceLastFrameSeconds >= _decoupleActivationInterval)
                    _recoupleFrames = 0;
                else
                {
                    _recoupleFrames++;

                    // We reached enough stable frames to activate the lock in again
                    if (_recoupleFrames >= _recoupleFrameCount)
                        _decoupleActive = false;
                }
            }
        }

        void ProcessConfigUpdates()
        {
            if (_postProcessing != null)
            {
                PostProcessingUpdated?.Invoke(_postProcessing);

                PackerMemoryPool.Instance.Return(_postProcessing);
                _postProcessing = null;
            }

            if (_quality != null)
            {
                ApplyQualityConfig(_quality);

                PackerMemoryPool.Instance.Return(_quality);
                _quality = null;
            }

            if (_resolution != null)
            {
                ApplyResolutionConfig(_resolution);

                PackerMemoryPool.Instance.Return(_resolution);
                _resolution = null;
            }

            UpdateDesktopRendering();
        }

        bool TryProcessFrame()
        {
            var frameDataToProcess = Interlocked.Exchange(ref _frameData, null);

            if (frameDataToProcess != null)
            {
                if (!ProcessFrameData(frameDataToProcess))
                    return false;

                processedFrameToNextBegin.Restart();

                return true;
            }
            else
                return false;
        }

        void HandleUpdate()
        {
            if (_initData != null)
            {
                StartCoroutine(HandleInit(_initData));
                _initData = null;
                return;
            }

            if (!_initFinalized)
                return;

            if (DebugFramePacing)
                Debug.Log($"{DateTime.Now.ToMillisecondTimeString()} SENDING BEGIN FRAME {LastFrameIndex}");

            Stats.Update();

            if (_lastFrameDataProcessed)
            {
                processedFrameToNextBegin.Stop();

                readyToFrameStopwatch.Restart();

                // Indicate that the frame is ready to start
                _primaryMessagingManager.SendCommand(BeginFrame());
            }
            else
                Input.UpdateStateDecoupled();

            RunAssetIntegration();

            // Check in case this didn't get to this part in the loop
            if (_fatalError)
                return;

            ProcessConfigUpdates();

            // Finally try to process the frame for render
            _lastFrameDataProcessed = TryProcessFrame();
        }

        bool ProcessFrameData(FrameSubmitData data)
        {
            try
            {
                processingStopwatch.Restart();

                if (DebugFramePacing)
                    Debug.Log($"{DateTime.Now.ToMillisecondTimeString()} PROCESSING FRAME {data.frameIndex}");

                HandleFrameUpdate(data);

                processingStopwatch.Stop();

                Stats.FrameUpdateHandleTime = (float)processingStopwatch.Elapsed.TotalSeconds;

                if (DebugFramePacing)
                    Debug.Log($"{DateTime.Now.ToMillisecondTimeString()} PROCESSED FRAME {data.frameIndex}");

                // Check if there's any render tasks. If there's any, we'll need to run those after the frame has finished
                if(data.renderTasks == null)
                    PackerMemoryPool.Instance.Return(data);
                else
                {
                    if (_dataWithRenderTasks != null)
                        throw new Exception($"There's an unprocessed data with render tasks");

                    _dataWithRenderTasks = data;
                }

            }
            catch (Exception ex)
            {
                _fatalError = true;

                Debug.LogError($"Exception handling frame update!\n{ex}" +
                    $"\nFrameData: {data?.ToString()}");

                // This is fatal, we can't continue execution
                Application.Quit(4);

                return false;
            }

            return true;
        }

        IEnumerator RenderTaskProcessor()
        {
            while(this != null)
            {
                yield return new WaitForEndOfFrame();

                if(_dataWithRenderTasks != null)
                {
                    ProcessRenderTasks(_dataWithRenderTasks.renderTasks);
                    PackerMemoryPool.Instance.Return(_dataWithRenderTasks);

                    _dataWithRenderTasks = null;
                }
            }
        }

        HeadOutput UpdateVR_Active(bool vrActive)
        {
            var activeOutput = vrActive ? _vrOutput : _screenOutput;
            var disabledOutput = vrActive ? _screenOutput : _vrOutput;

            if (disabledOutput != null)
            {
                if (disabledOutput.gameObject.activeSelf)
                    disabledOutput.gameObject.SetActive(false);
            }

            if (!activeOutput.gameObject.activeSelf)
                activeOutput.gameObject.SetActive(true);

            if(vrActive != _lastVRactive)
            {
                _lastVRactive = vrActive;

                Input.VR_ActiveChanged(vrActive);

                UpdateQualitySettings(vrActive);
            }           

            return activeOutput;
        }

        void UpdateQualitySettings(bool vrActive)
        {
            if (_lastVRactive.Value)
            {
                QualitySettings.lodBias = 3.8f;
                QualitySettings.maxQueuedFrames = 0;
            }
            else
            {
                QualitySettings.lodBias = 2f;
                QualitySettings.maxQueuedFrames = 2;
            }
        }

        void ApplyQualityConfig(QualityConfig config)
        {
            QualitySettings.pixelLightCount = config.perPixelLights;

            QualitySettings.shadowResolution = config.shadowResolution.ToUnity();
            QualitySettings.shadowCascades = config.shadowCascades.ToUnity();
            QualitySettings.shadowDistance = config.shadowDistance;

            QualitySettings.skinWeights = config.skinWeightMode.ToUnity();
        }

        void ApplyResolutionConfig(ResolutionConfig config)
        {
            if (config.resolution.x != Screen.width || config.resolution.y != Screen.height)
                Screen.SetResolution(config.resolution.x, config.resolution.y, config.fullscreen);
            else
                Screen.fullScreen = config.fullscreen;

            Window.FlagResolutionChanged();
        }

        void UpdateDesktopRendering()
        {
            // We can't control these settings if VR is active as that will interfere
            if (_vrOutput != null)
                return;

            int? maxFramerate = Window.IsFocused ? _maxForegroundFPS : _maxBackgroundFPS;

            if(maxFramerate != null)
            {
                Application.targetFrameRate = Math.Max(5, maxFramerate.Value);
                QualitySettings.vSyncCount = 0;
            }
            else
            {
                Application.targetFrameRate = -1;
                QualitySettings.vSyncCount = _useVSync ? 1 : 0;
            }
        }

        void HandleRenderCommand(RendererCommand command, int messageSize)
        {
            if (IsDebug)
                Debug.Log("Received command: " + command + " - Size: " + messageSize);

            if (command is KeepAlive)
                return;

            // We must always receive init first
            if (!_initReceived)
            {
                if (!(command is RendererInitData initData))
                    throw new InvalidOperationException($"{nameof(RendererInitData)} must be the first message");

                // We're on another thread so we can't actually process the init immediately, store it for later
                _initData = initData;

                // Indicate that we've received init - this way we know if another one is sent even if we processed this one
                _initReceived = true;
                return;
            }

            if (command is RendererShutdown shutdown)
            {
                _shutdown = true;
                _processingReady?.Set();
                return;
            }

            // We can process initial asset uploads before init finalizes
            switch (command)
            {
                case SetWindowIcon setIcon:
                    HandleSetIcon(setIcon);
                    return;

                case FreeSharedMemoryView freeSharedMemory:
                    SharedMemory.ReleaseView(freeSharedMemory.bufferId);
                    PackerMemoryPool.Instance.Return(freeSharedMemory);
                    return;

                case RendererParentWindow parentWindow:
                    var result = WindowsNativeHelper.ParentWindowUnderMain(new IntPtr(parentWindow.windowHandle));
                    Debug.Log($"Parenting window: 0x{parentWindow.windowHandle:X} - success: {result}");
                    return;

                case SetTaskbarProgress taskbarProgress:
                    HandleTaskbarProgress(taskbarProgress);
                    return;

                // Assets
                case MeshUploadData meshUpload:
                    Meshes.GetAsset(meshUpload.assetId).Handle(meshUpload);
                    return;

                case MeshUnload meshUnload:
                    Meshes.GetAsset(meshUnload.assetId).Handle(meshUnload);
                    return;

                case ShaderUpload shaderUpload:
                    Shaders.GetAsset(shaderUpload.assetId).Handle(shaderUpload);
                    return;

                case ShaderUnload shaderUnload:
                    Shaders.GetAsset(shaderUnload.assetId).Handle(shaderUnload);
                    return;

                case MaterialPropertyIdRequest materialPropertyRequest:
                    HandleMaterialPropertyRequest(materialPropertyRequest);
                    return;

                case MaterialsUpdateBatch materialUpdateBatch:
                    Materials.Handle(materialUpdateBatch);
                    return;

                case UnloadMaterial unloadMaterial:
                    Materials.Handle(unloadMaterial);
                    return;

                case UnloadMaterialPropertyBlock unloadPropertyBlock:
                    Materials.Handle(unloadPropertyBlock);
                    return;

                case SetTexture2DFormat setTexture2Dformat:
                    Texture2Ds.GetAsset(setTexture2Dformat.assetId).SetFormat(setTexture2Dformat);
                    return;

                case SetTexture2DProperties setTexture2Dproperties:
                    Texture2Ds.GetAsset(setTexture2Dproperties.assetId).SetProperties(setTexture2Dproperties);
                    return;

                case SetTexture2DData setTexture2Ddata:
                    Texture2Ds.GetAsset(setTexture2Ddata.assetId).SetData(setTexture2Ddata);
                    return;

                case UnloadTexture2D unloadTexture2D:
                    Texture2Ds.GetAsset(unloadTexture2D.assetId).Unload();
                    PackerMemoryPool.Instance.Return(unloadTexture2D);
                    return;

                case SetDesktopTextureProperties setDesktopTextureProperties:
                    DesktopTextures.GetAsset(setDesktopTextureProperties.assetId).Handle(setDesktopTextureProperties);
                    return;

                case UnloadDesktopTexture unloadDesktopTexture:
                    DesktopTextures.GetAsset(unloadDesktopTexture.assetId).Unload();
                    PackerMemoryPool.Instance.Return(unloadDesktopTexture);
                    return;

                case SetTexture3DFormat setTexture3Dformat:
                    Texture3Ds.GetAsset(setTexture3Dformat.assetId).SetFormat(setTexture3Dformat);
                    return;

                case SetTexture3DProperties setTexture3Dproperties:
                    Texture3Ds.GetAsset(setTexture3Dproperties.assetId).SetProperties(setTexture3Dproperties);
                    return;

                case SetTexture3DData setTexture3Ddata:
                    Texture3Ds.GetAsset(setTexture3Ddata.assetId).SetData(setTexture3Ddata);
                    return;

                case UnloadTexture3D unloadTexture3D:
                    Texture3Ds.GetAsset(unloadTexture3D.assetId).Unload();
                    PackerMemoryPool.Instance.Return(unloadTexture3D);
                    return;

                case SetCubemapFormat setCubemapformat:
                    Cubemaps.GetAsset(setCubemapformat.assetId).SetFormat(setCubemapformat);
                    return;

                case SetCubemapProperties setCubemapproperties:
                    Cubemaps.GetAsset(setCubemapproperties.assetId).SetProperties(setCubemapproperties);
                    return;

                case SetCubemapData setCubemapdata:
                    Cubemaps.GetAsset(setCubemapdata.assetId).SetData(setCubemapdata);
                    return;

                case UnloadCubemap unloadCubemap:
                    Cubemaps.GetAsset(unloadCubemap.assetId).Unload();
                    PackerMemoryPool.Instance.Return(unloadCubemap);
                    return;

                case SetRenderTextureFormat setRenderTextureFormat:
                    RenderTextures.GetAsset(setRenderTextureFormat.assetId).Handle(setRenderTextureFormat);
                    return;

                case UnloadRenderTexture unloadRenderTexture:
                    RenderTextures.GetAsset(unloadRenderTexture.assetId).Handle(unloadRenderTexture);
                    return;

                case VideoTextureLoad videoLoad:
                    VideoTextures.GetAsset(videoLoad.assetId).Handle(videoLoad);
                    return;

                case VideoTextureUpdate videoUpdate:
                    VideoTextures.GetAsset(videoUpdate.assetId).Handle(videoUpdate);
                    return;

                case VideoTextureProperties videoProperties:
                    VideoTextures.GetAsset(videoProperties.assetId).Handle(videoProperties);
                    return;

                case VideoTextureStartAudioTrack videoAudioStart:
                    VideoTextures.GetAsset(videoAudioStart.assetId).Handle(videoAudioStart);
                    return;

                case UnloadVideoTexture videoUnload:
                    VideoTextures.GetAsset(videoUnload.assetId).Unload();
                    PackerMemoryPool.Instance.Return(videoUnload);
                    return;

                case PointRenderBufferUpload pointRenderBufferUpload:
                    PointRenderBuffers.GetAsset(pointRenderBufferUpload.assetId).HandleUpload(pointRenderBufferUpload);
                    return;

                case PointRenderBufferUnload pointRenderBufferUnload:
                    PointRenderBuffers.GetAsset(pointRenderBufferUnload.assetId).HandleUnload(pointRenderBufferUnload);
                    return;

                case TrailRenderBufferUpload trailRenderBufferUpload:
                    TrailsRenderBuffers.GetAsset(trailRenderBufferUpload.assetId).HandleUpload(trailRenderBufferUpload);
                    return;

                case TrailRenderBufferUnload trailRenderBufferUnload:
                    TrailsRenderBuffers.GetAsset(trailRenderBufferUnload.assetId).HandleUnload(trailRenderBufferUnload);
                    return;

                case GaussianSplatUpload gaussianSplatUpload:
                    GaussianSplats.GetAsset(gaussianSplatUpload.assetId).HandleUpload(gaussianSplatUpload);
                    return;

                case UnloadGaussianSplat unloadGaussianSplat:
                    GaussianSplats.GetAsset(unloadGaussianSplat.assetId).Unload();
                    PackerMemoryPool.Instance.Return(unloadGaussianSplat);
                    return;

                case LightsBufferRendererSubmission lightsBufferSubmission:
                    var buffer = TryGetLightsBuffer(lightsBufferSubmission.lightsBufferUniqueId);

                    if (buffer != null)
                        buffer.HandleSubmission(lightsBufferSubmission);
                    else
                    {
                        // Send submitted immediately
                        var consumed = new LightsBufferRendererConsumed();
                        consumed.globalUniqueId = lightsBufferSubmission.lightsBufferUniqueId;

                        SendBufferConsumed(consumed);

                        PackerMemoryPool.Instance.Return(lightsBufferSubmission);
                    }

                    return;
            }

            if (!_initFinalized)
            {
                switch (command)
                {
                    case RendererInitProgressUpdate progressUpdate:
                        Progress.UpdateProgress(progressUpdate);
                        return;

                    case RendererInitFinalizeData initFinalize:
                        HandleInitFinalize(initFinalize);
                        return;

                    default:
                        throw new InvalidOperationException("Invalid message type while waiting for init to finalize: " + command.GetType());
                }

                return;
            }

            switch (command)
            {
                case RendererEngineReady engineReady:
                    HandleEngineReady(engineReady);
                    return;

                case FrameSubmitData frameSubmission:
                    readyToFrameStopwatch.Stop();

                    _frameData = frameSubmission;

                    if (DebugFramePacing)
                        Debug.Log($"{DateTime.Now.ToMillisecondTimeString()} FRAME SUBMISSION RECEIVE: {frameSubmission.frameIndex}");

                    _processingReady.Set();
                    return;

                case PostProcessingConfig postProcessing:
                    _postProcessing = postProcessing;
                    return;

                case QualityConfig quality:
                    _quality = quality;
                    return;

                case ResolutionConfig resolution:
                    _resolution = resolution;
                    return;

                case DesktopConfig desktop:
                    _maxBackgroundFPS = desktop.maximumBackgroundFramerate;
                    _maxForegroundFPS = desktop.maximumForegroundFramerate;
                    _useVSync = desktop.vSync;

                    PackerMemoryPool.Instance.Return(desktop);
                    return;

                case RenderDecouplingConfig decouple:
                    _decoupleActivationInterval = decouple.decoupleActivateInterval;
                    _decoupledMaxAssetProcessingTime = decouple.decoupledMaxAssetProcessingTime;
                    _recoupleFrames = decouple.recoupleFrameCount;
                    return;

                case GaussianSplatConfig gaussianSplatConfig:
                    GaussianSplatRendererManager.ApplyConfig(gaussianSplatConfig);
                    return;

                default:
                    throw new InvalidOperationException("Invalid message type: " + command.GetType());
            }
        }

        void HandleMaterialPropertyRequest(MaterialPropertyIdRequest request)
        {
            // We can handle this instantly since this is thread-safe API call
            var result = new MaterialPropertyIdResult();
            result.requestId = request.requestId;

            for (int i = 0; i < request.propertyNames.Count; i++)
                result.propertyIDs.Add(Shader.PropertyToID(request.propertyNames[i]));

            // Send it back
            _backgroundMessagingManager.SendCommand(result);
        }

        void HandleMessagingFailure(Exception ex)
        {
            // We want to force quit with nothing stopping it
            _fatalError = true;

            Debug.LogError($"Exception in messaging system:\n" + ex);

            Application.Quit(3);

            // Make sure the frame gets unstuck
            _processingReady.Set();
        }

        IEnumerator HandleInit(RendererInitData initData)
        {
            UniqueSessionId = initData.uniqueSessionId;

            Debug.Log("UniqueSessionId: " + UniqueSessionId);

            if (!Renderite.Shared.Helper.IsWine)
            {
                var wasapi = new CSCore.SoundOut.WasapiOut(false, CSCore.CoreAudioAPI.AudioClientShareMode.Shared, 100, initData.uniqueSessionId, true);
                var audioFormat = new WaveFormat(wasapi.Device.DeviceFormat.SampleRate, 32, wasapi.Device.DeviceFormat.Channels, AudioEncoding.IeeeFloat);
                wasapi.Initialize(new SilenceSource(audioFormat));

                Debug.Log($"Initialized dummy WASAPI session");
            }

            if (initData.windowTitle != null)
            {
                if (WindowsNativeHelper.SetWindowTitle(initData.windowTitle))
                    Debug.Log($"Set window title to {initData.windowTitle}");
                else
                    Debug.LogWarning("Failed to set window title");
            }
            else
                Debug.Log("No window title was provided");

            // Allocate reset event for synchronization
            _processingReady = new ManualResetEventSlim(false);

            SharedMemory = new SharedMemoryAccessor(initData.sharedMemoryPrefix);

            _mainProcessId = initData.mainProcessId;

            if (!Renderite.Shared.Helper.IsWine)
                MainProcess = System.Diagnostics.Process.GetProcessById(_mainProcessId);

            Task.Run(MainProcessWatchDog);

            DebugFramePacing = initData.debugFramePacing;

            AssetIntegrator = new AssetIntegrator();
            AssetIntegrator.Initialize(() => _processingReady.Set());

            Results = new FrameResultsManager();
            Stats = new PerformanceStats();

            Input = new InputManager(Mouse, Keyboard, Window, Display, InputDrivers);

            Texture2Ds = new AssetManager<Texture2DAsset>();
            Texture3Ds = new AssetManager<Texture3DAsset>();
            Cubemaps = new AssetManager<CubemapAsset>();
            RenderTextures = new AssetManager<RenderTextureAsset>();
            VideoTextures = new AssetManager<VideoTextureAsset>();
            DesktopTextures = new AssetManager<DesktopTextureAsset>();
            Meshes = new AssetManager<MeshAsset>();
            Shaders = new AssetManager<ShaderAsset>();
            Materials = new MaterialAssetManager();
            PointRenderBuffers = new AssetManager<PointRenderBufferAsset>();
            TrailsRenderBuffers = new AssetManager<TrailsRenderBufferAsset>();
            GaussianSplats = new AssetManager<GaussianSplatAsset>();

            // Handle the icon if present
            if (initData.setWindowIcon != null)
            {
                Debug.Log("Setting renderer icon");
                HandleSetIcon(initData.setWindowIcon);
            }

            // Handle splash screen override if present
            if (initData.splashScreenOverride != null)
            {
                Debug.Log("Applying splash screen override");
                Progress.ApplySplashScreenOverride(initData.splashScreenOverride);
            }

            Progress.InitStarted();

            if (initData.outputDevice == HeadOutputDevice.Autodetect)
                yield return AutodetectOutputDevice(initData);

            yield return LoadOutputDevice(initData.outputDevice);

            var initializedDevice = InitializeHeadOutputs(initData.outputDevice);

            var result = new RendererInitResult();

            result.rendererIdentifier = $"Renderite.Renderer.Unity {Application.version} ({Application.unityVersion})";
            result.mainWindowHandlePtr = WindowsNativeHelper.MainWindowHandle.ToInt64();

            result.actualOutputDevice = initializedDevice;
            result.stereoRenderingMode = XRSettings.stereoRenderingMode.ToString();
            result.maxTextureSize = SystemInfo.maxTextureSize;

            var graphicsDeviceType = SystemInfo.graphicsDeviceType;

            switch (graphicsDeviceType)
            {
                case UnityEngine.Rendering.GraphicsDeviceType.Direct3D11:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                    result.isGPUTexturePOTByteAligned = true;
                    break;
            }

            result.supportedTextureFormats = new List<Shared.TextureFormat>();

            foreach (var format in Enums.GetValues<Renderite.Shared.TextureFormat>())
            {
                if (graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11)
                {
                    // Check if the format can be converted to DX11
                    if (format.TryToDX11(ColorProfile.Linear, true) == null)
                        continue;
                }
                else
                {
                    var unityFormat = format.ToUnity(false);

                    if (unityFormat < 0)
                        continue;

                    if (!SystemInfo.SupportsTextureFormat(unityFormat))
                        continue;
                }

                result.supportedTextureFormats.Add(format);
            }

            // Indicate that we finished init
            _primaryMessagingManager.SendCommand(result);
        }

        void HandleInitFinalize(RendererInitFinalizeData initFinalize)
        {
            _initFinalized = true;
        }

        void HandleEngineReady(RendererEngineReady engineReady)
        {
            // Indicate that it's completed
            Progress.InitCompleted();

            // Start waiting for the frame data to ensure lockstep
            _lockStepActivated = true;
        }

        void HandleFrameUpdate(FrameSubmitData submitData)
        {
            if (submitData.debugLog)
                Debug.Log($"DEBUG LOG: {submitData.ToString()}");

            LastFrameIndex = submitData.frameIndex;

            NearClip = submitData.nearClip;
            FarClip = submitData.farClip;
            DesktopFOV = submitData.desktopFOV;

            // Update all the render spaces
            RenderSpace _activeSpace = null;

            foreach (var renderSpace in submitData.renderSpaces)
            {
                if (!_renderSpaces.TryGetValue(renderSpace.id, out var space))
                {
                    // Allocate a new space
                    var go = new GameObject($"RenderSpace: {renderSpace.id}");

                    space = go.AddComponent<RenderSpace>();
                    space.Initialize(renderSpace.id);

                    _renderSpaces.Add(renderSpace.id, space);
                }

                space.HandleUpdate(renderSpace);

                if (renderSpace.isActive && !renderSpace.isOverlay)
                {
                    if (_activeSpace != null)
                        throw new InvalidOperationException($"Trying to set multiple active render spaces. Exiting active: {_activeSpace}, second active: {space}");

                    _activeSpace = space;
                }
            }

            // Update active output and positioning
            var activeOutput = UpdateVR_Active(submitData.vrActive);

            // Update positioning of the active output
            if (_activeSpace != null)
                activeOutput.UpdatePositioning(_activeSpace);

            foreach (var space in _renderSpaces)
            {
                // We only care about active overlays
                if (!space.Value.IsActive || !space.Value.IsOverlay)
                    continue;

                space.Value.UpdateOverlayPositioning(activeOutput.transform);
            }

            // Remove render spaces that were not updated
            foreach (var space in _renderSpaces)
            {
                if (space.Value.WasUpdated)
                {
                    space.Value.ClearUpdated();
                    continue;
                }

                _spacesToRemove.Add(space.Key);
            }

            foreach (var space in _spacesToRemove)
            {
                _renderSpaces[space].Remove();
                _renderSpaces.Remove(space);
            }

            _spacesToRemove.Clear();

            if (submitData.outputState != null)
                Input.HandleOutputState(submitData.outputState);
        }

        void ProcessRenderTasks(List<CameraRenderTask> renderTasks)
        {
            var previousContext = RenderContextHelper.CurrentRenderingContext;
            RenderContextHelper.BeginRenderContext(RenderingContext.RenderToAsset);

            foreach (var task in renderTasks)
                CameraRenderer.Render(task);

            // Make sure to always end the last render context
            if (previousContext != null)
                RenderContextHelper.BeginRenderContext(previousContext.Value);
            else
                RenderContextHelper.EndCurrentRenderContext();
        }

        bool GetConnectionParameters(out string queueName, out long queueCapacity)
        {
            if (Application.isEditor)
            {
                queueName = EditorQueueName;
                queueCapacity = EditorQueueCapacity;
                return true;
            }

            var args = System.Environment.GetCommandLineArgs();

            queueName = null;
            queueCapacity = -1;

            if (args == null || args.Length == 0)
                return false;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                // Check if there's a next one
                var next = i + 1;

                if (next >= args.Length)
                    return false;

                if (arg.EndsWith(Renderite.Shared.Helper.QUEUE_NAME_ARGUMENT, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Cannot have two definitions
                    if (queueName != null)
                        return false;

                    queueName = args[next];

                    // Skip the next
                    i++;
                }
                else if (arg.EndsWith(Renderite.Shared.Helper.QUEUE_CAPACITY_ARGUMENT, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Can't have two definitions either
                    if (queueCapacity > 0)
                        return false;

                    if (!long.TryParse(args[next], out queueCapacity))
                        return false;

                    // Skip the next
                    i++;
                }

                // Check if we got both now
                if (queueName != null && queueCapacity > 0)
                    return true;
            }

            // Didn't find the necessary arguments, bail
            return false;
        }

        // This is a bit nasty way to shut the process down, but unfortunately due to a Unity bug it's the only way I know of
        // that is relatively "clean". Normal shutdown results in a crash dialog, due to us hooking into the rendering events.
        // This somehow corrupts the state of Mono on shutdown.
        void ForceCrash() => System.Diagnostics.Process.GetCurrentProcess().Kill();

        bool OnAppWantsToQuit()
        {
            Debug.Log($"AppWantsToQuit. InitStarted: {_initReceived}, InitFinalized: {_initFinalized}, FatalError: {_fatalError}, Shutdown: {_shutdown}");

            Debug.Log("=================================================================== LOG END ===================================================================");

            // If we haven't finalized the init, we can shutdown cleanly still, because the GPU event wasn't hooked yet
            if (!_initFinalized)
                return true;

            if (_fatalError || _shutdown)
            {
                if (!Application.isEditor)
                    ForceCrash();

                return true;
            }

            // Send the shutdown request
            _primaryMessagingManager.SendCommand(new RendererShutdownRequest());

            return false;
        }

        public RenderSpace TryGetRenderSpace(int renderSpaceId)
        {
            if (_renderSpaces.TryGetValue(renderSpaceId, out var space))
                return space;

            return null;
        }

        public void Register(LightsBufferRenderer renderer)
        {
            if (renderer.GlobalUniqueId < 0)
                throw new ArgumentException("Renderer doesn't have assigned global unique ID");

            lock (_lightBuffers)
                _lightBuffers.Add(renderer.GlobalUniqueId, renderer);
        }

        public void Unregister(LightsBufferRenderer renderer)
        {
            if (renderer.GlobalUniqueId < 0)
                throw new ArgumentException("Renderer doesn't have assigned global unique ID");

            lock (_lightBuffers)
                _lightBuffers.Remove(renderer.GlobalUniqueId);
        }

        public LightsBufferRenderer TryGetLightsBuffer(int uniqueId)
        {
            lock (_lightBuffers)
            {
                if (_lightBuffers.TryGetValue(uniqueId, out var renderer))
                    return renderer;
                else
                    return null;
            }
        }

        public void SendReflectionProbeRenderResult(ReflectionProbeRenderResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result.renderTaskId < 0)
                throw new ArgumentException($"{nameof(result.renderTaskId)} was not set");

            _backgroundMessagingManager.SendCommand(result);
        }

        public void SendAssetUpdate(AssetCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _backgroundMessagingManager.SendCommand(command);
        }

        public void SendMaterialUpdateResult(MaterialsUpdateBatchResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result.updateBatchId < 0)
                throw new ArgumentException("UpdateBatchId was not initialized");

            _backgroundMessagingManager.SendCommand(result);
        }

        public void SendBufferConsumed(LightsBufferRendererConsumed consumed)
        {
            if (consumed == null)
                throw new ArgumentNullException(nameof(consumed));

            _backgroundMessagingManager.SendCommand(consumed);
        }

        IEnumerator AutodetectOutputDevice(RendererInitData initData)
        {
            var devices = new List<string>();

            devices.Add("oculus");
            devices.Add("openvr");
            devices.Add("none");

            if (System.Diagnostics.Process.GetProcessesByName("vrcompositor").Length > 0 &&
                System.Diagnostics.Process.GetProcessesByName("vrmonitor").Length > 0)
            {
                Debug.Log("Detected SteamVR running, skipping Oculus Runtime initialization.");
                devices.Remove("oculus");
            }

            XRSettings.LoadDeviceByName(devices.ToArray());

            yield return null;

            XRSettings.enabled = true;

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    if (XRDevice.isPresent)
                        initData.outputDevice = HeadOutputDevice.OculusQuest;
                    else
                        initData.outputDevice = HeadOutputDevice.Screen;
                    break;

                // assume a PC platform
                default:
                    if (XRDevice.isPresent)
                    {
                        if (XRSettings.loadedDeviceName.ToLower().Contains("oculus"))
                            initData.outputDevice = HeadOutputDevice.Oculus;
                        else
                            initData.outputDevice = HeadOutputDevice.SteamVR;
                    }
                    else
                        initData.outputDevice = HeadOutputDevice.Screen;
                    break;
            }

            Debug.Log("Autodetected device: " + initData.outputDevice);
        }

        IEnumerator LoadOutputDevice(HeadOutputDevice device)
        {
            Debug.Log("Loading output device: " + device);

            switch (device)
            {
                case HeadOutputDevice.Oculus:
                    yield return LoadDevice("oculus");
                    break;

                case HeadOutputDevice.OculusQuest:
                    // do not do anything for these, as they implicitly use the VR mode
                    break;

                case HeadOutputDevice.SteamVR:
                case HeadOutputDevice.WindowsMR:
                    yield return LoadDevice("openvr");
                    break;
            }
        }

        IEnumerator LoadDevice(string newDevice)
        {
            Debug.Log("Loading XR runtime: " + newDevice);

            if (string.Compare(UnityEngine.XR.XRSettings.loadedDeviceName, newDevice, true) != 0)
            {
                XRSettings.LoadDeviceByName(newDevice);
                yield return null;
                XRSettings.enabled = true;
            }
        }

        HeadOutputDevice InitializeHeadOutputs(HeadOutputDevice device)
        {
            // create head output first
            if (device.IsScreenViewSupported())
            {
                HeadOutputDevice screenRenderer;

                if (device == HeadOutputDevice.Screen360)
                    screenRenderer = device;
                else
                    screenRenderer = HeadOutputDevice.Screen;

                _screenOutput = HeadOutput.GetHeadObject(screenRenderer);

                RegisterInputDrivers(_screenOutput.gameObject);
            }
            else
            {
                // no overlay
                Destroy(OverlayCamera.gameObject);
            }

            if (device.IsVR())
                _vrOutput = HeadOutput.GetHeadObject(device);

            if (_vrOutput != null && _screenOutput != null)
            {
                // Setup headset driver
                var driverHeadDevice = _vrOutput.GetComponentInChildren<IDriverHeadDevice>();

                // get the device override
                if (driverHeadDevice != null)
                {
                    device = driverHeadDevice.Device;

                    RegisterInputDrivers(_vrOutput.gameObject);
                }

                if (_screenOutput != null)
                    _screenOutput.gameObject.SetActive(false);
            }

            return device;
        }

        void RegisterInputDrivers(GameObject root)
        {
            foreach (var driver in root.GetComponentsInChildren<InputDriver>())
                Input.RegisterDriver(driver);
        }

        void HandleSetIcon(SetWindowIcon icon)
        {
            var expectedSize = icon.size.x * icon.size.y * 4;

            if (icon.iconData.length != expectedSize)
                throw new ArgumentException($"Indicated icon size is {icon.size}, expected {expectedSize} bytes for icon data, got: {icon.iconData.length}");

            var iconData = SharedMemory.AccessData(icon.iconData);

            bool success;

            if (!icon.isOverlay)
                success = WindowIconTools.SetIcon(iconData, icon.size.x, icon.size.y, WindowIconKind.Big);
            else
                success = WindowIconTools.SetOverlayIcon(iconData, icon.size.x, icon.size.y, icon.overlayDescription ?? "");

            // Send response
            var response = new SetWindowIconResult();

            response.success = success;
            response.requestId = icon.requestId;

            _backgroundMessagingManager.SendCommand(response);
        }

        void HandleTaskbarProgress(SetTaskbarProgress progress)
        {
            WindowIconTools.SetProgress(progress.mode switch
            {
                TaskbarProgressBarMode.None => TaskbarProgressBarState.NoProgress,
                TaskbarProgressBarMode.Normal => TaskbarProgressBarState.Normal,
                TaskbarProgressBarMode.Indeterminate => TaskbarProgressBarState.Indeterminate,
                TaskbarProgressBarMode.Paused => TaskbarProgressBarState.Paused,
                TaskbarProgressBarMode.Error => TaskbarProgressBarState.Error,
                _ => throw new ArgumentException($"Invalid mode: {progress.mode}")
            }, progress.completed, progress.total);

            PackerMemoryPool.Instance.Return(progress);
        }

        async Task MainProcessWatchDog()
        {
            // Under Linux, the renderer will be started in wine/proton which
            // doesn't understand linux PIDs. However, linux processes are listed
            // as directories under /proc/(PID). Since this is just presented as
            // a directory, it's also accessible via wine and can be used to
            // determine if the process is still running.

            while (!_shutdown)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                if (HasMainProcessExited && !_shutdown)
                {
                    Debug.Log("Main process has exited. Shutting down");
                    ForceCrash();

                    // Why even bother?
                    break;
                }
            }
        }
    }
}
