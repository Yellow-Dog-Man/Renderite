using System;
using System.Collections.Generic;
using System.Text;
using Elements.Data;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.Key", "FrooxEngine")]
    public enum Key
    {
        None = 0,
        Backspace = 8,
        Tab = 9,
        Clear = 12,
        Return = 13,
        Pause = 19,
        Escape = 27,
        Space = 32,
        Exclaim = 33,
        DoubleQuote = 34,
        Hash = 35,
        Dollar = 36,
        Ampersand = 38,
        Quote = 39,
        LeftParenthesis = 40,
        RightParenthesis = 41,
        Asterisk = 42,
        Plus = 43,
        Comma = 44,
        Minus = 45,
        Period = 46,
        Slash = 47,
        Alpha0 = 48,
        Alpha1 = 49,
        Alpha2 = 50,
        Alpha3 = 51,
        Alpha4 = 52,
        Alpha5 = 53,
        Alpha6 = 54,
        Alpha7 = 55,
        Alpha8 = 56,
        Alpha9 = 57,
        Colon = 58,
        Semicolon = 59,
        Less = 60,
        Equals = 61,
        Greater = 62,
        Question = 63,
        At = 64,
        VerticalBar = 65,
        LeftBracket = 91,
        Backslash = 92,
        RightBracket = 93,
        Caret = 94,
        Underscore = 95,
        BackQuote = 96,
        A = 97,
        B = 98,
        C = 99,
        D = 100,
        E = 101,
        F = 102,
        G = 103,
        H = 104,
        I = 105,
        J = 106,
        K = 107,
        L = 108,
        M = 109,
        N = 110,
        O = 111,
        P = 112,
        Q = 113,
        R = 114,
        S = 115,
        T = 116,
        U = 117,
        V = 118,
        W = 119,
        X = 120,
        Y = 121,
        Z = 122,
        Percent = 123,
        Tilde = 124,
        LeftBrace = 125,
        RightBrace = 126,
        Delete = 127,
        Keypad0 = 256,
        Keypad1 = 257,
        Keypad2 = 258,
        Keypad3 = 259,
        Keypad4 = 260,
        Keypad5 = 261,
        Keypad6 = 262,
        Keypad7 = 263,
        Keypad8 = 264,
        Keypad9 = 265,
        KeypadPeriod = 266,
        KeypadDivide = 267,
        KeypadMultiply = 268,
        KeypadMinus = 269,
        KeypadPlus = 270,
        KeypadEnter = 271,
        KeypadEquals = 272,
        UpArrow = 273,
        DownArrow = 274,
        RightArrow = 275,
        LeftArrow = 276,
        Insert = 277,
        Home = 278,
        End = 279,
        PageUp = 280,
        PageDown = 281,
        F1 = 282,
        F2 = 283,
        F3 = 284,
        F4 = 285,
        F5 = 286,
        F6 = 287,
        F7 = 288,
        F8 = 289,
        F9 = 290,
        F10 = 291,
        F11 = 292,
        F12 = 293,
        F13 = 294,
        F14 = 295,
        F15 = 296,
        Numlock = 300,
        CapsLock = 301,
        ScrollLock = 302,
        RightShift = 303,
        LeftShift = 304,
        RightControl = 305,
        LeftControl = 306,
        RightAlt = 307,
        LeftAlt = 308,
        RightApple = 309,
        RightCommand = 309,
        LeftApple = 310,
        LeftCommand = 310,
        LeftWindows = 311,
        RightWindows = 312,
        AltGr = 313,
        Help = 315,
        Print = 316,
        SysReq = 317,
        Break = 318,
        Menu = 319,

        // Combined keys
        Shift = 512,
        Control = 513,
        Alt = 514,
        Windows = 515
    }

    public static class KeyHelper
    {
        public static bool IsModifier(this Key key) => key.IsShift() || key.IsAlt() || key.IsWin() || key.IsCtrl();

        public static bool IsShift(this Key key) => key == Key.LeftShift || key == Key.RightShift || key == Key.Shift;
        public static bool IsAlt(this Key key) => key == Key.LeftAlt || key == Key.RightAlt || key == Key.AltGr || key == Key.Alt;
        public static bool IsWin(this Key key) => key == Key.LeftWindows || key == Key.RightWindows || key == Key.Windows;
        public static bool IsCtrl(this Key key) => key == Key.LeftControl || key == Key.RightControl || key == Key.Control;

        public static Key ToKey(this char ch, out bool shift)
        {
            shift = char.IsUpper(ch);

            if (shift)
                ch = char.ToLower(ch);

            if (ch >= 'a' && ch <= 'z')
                return Key.A + (ch - 'a');

            if (ch >= '0' && ch <= '9')
                return Key.Alpha0 + (ch - '0');

            return ch switch
            {
                '\t' => Key.Tab,
                '\n' => Key.Return,
                '\r' => Key.Return,
                '\b' => Key.Backspace,
                ' ' => Key.Space,
                '!' => Key.Exclaim,
                '"' => Key.DoubleQuote,
                '#' => Key.Hash,
                '$' => Key.Dollar,
                '&' => Key.Ampersand,
                '\'' => Key.Quote,
                '(' => Key.LeftParenthesis,
                ')' => Key.RightParenthesis,
                '*' => Key.Asterisk,
                '+' => Key.Plus,
                ',' => Key.Comma,
                '-' => Key.Minus,
                '.' => Key.Period,
                '/' => Key.Slash,
                ':' => Key.Colon,
                ';' => Key.Semicolon,
                '<' => Key.Less,
                '=' => Key.Equals,
                '>' => Key.Greater,
                '?' => Key.Question,
                '@' => Key.At,
                '[' => Key.LeftBracket,
                ']' => Key.RightBracket,
                '\\' => Key.Backslash,
                '^' => Key.Caret,
                '_' => Key.Underscore,
                '`' => Key.BackQuote,
                '%' => Key.Percent,
                '~' => Key.Tilde,
                '{' => Key.LeftBrace,
                '}' => Key.RightBrace,

                _ => Key.None
            };
        }
    }
}
