# The '--coverlet' switch should be passed automatically as long as 'TestingPlatformCommandLineArguments' was set.

$reportDirectory = Join-Path $PSScriptRoot "TestResults" "CoverageReports"
$reportTypes = "Html,TextSummary"

dotnet test

if ($LASTEXITCODE -ne 0) {
	exit $LASTEXITCODE
}

# Obtain latest coverage files for each project.

$coverageProjectDirs = Get-ChildItem -Path $PSScriptRoot -Filter "coverage.cobertura*.xml" -File -Recurse | Group-Object DirectoryName

$coverageFiles =  $coverageProjectDirs | ForEach-Object {
	$_.Group | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
}

if (-not $coverageFiles) {
	throw "No Cobertura files were found. Unable to generate coverage report."
}

# Generate a visual, human readable report with ReportGenerator.

$reports = $coverageFiles.FullName -join ";"

dotnet reportgenerator "-reports:$reports" "-targetDir:$reportDirectory" "-reporttypes:$reportTypes" "-verbosity:Warning"

if ($LASTEXITCODE -ne 0) {
	exit $LASTEXITCODE
}

# Replace string instances of CRAP, which stands for Change Risk Anti-Patterns, to "Change Risk Score" instead.

Get-ChildItem $reportDirectory -Include "*.htm", "*.html", "*.js"  -Recurse -File |
	ForEach-Object {
		$content = Get-Content $_.FullName -Raw

		$content = [regex]::Replace($content, "CRAP Score", "Change Risk Score", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

		Out-File -FilePath $_.FullName -InputObject $content
	}

# Output the text summary and the location of the HTML file.

Write-Host $(Get-Content (Join-Path $reportDirectory "Summary.txt") -Raw)
Write-Host

Write-Host "HTML report path: $(Join-Path $reportDirectory "index.html")"
Write-Host