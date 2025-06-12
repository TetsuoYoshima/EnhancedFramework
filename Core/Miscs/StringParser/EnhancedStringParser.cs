// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedFramework.Core {
    /// <summary>
    /// Base class to inherit your own <see cref="string"/> parsers from.
    /// </summary>
    public abstract class EnhancedStringParser {
        #region Constructor
        /// <summary>
        /// Prevents inheriting from this class in other assemblies.
        /// </summary>
        internal protected EnhancedStringParser() { }
        #endregion

        #region Parsing
        /// <summary>
        /// Tries to identify a <see cref="string"/> tag for this parser.
        /// </summary>
        /// <param name="_input"><see cref="StringBuilder"/> content.</param>
        /// <param name="_startIndex">Tag start index.</param>
        /// <param name="_endIndex">Tag end index.</param>
        /// <returns>True if the tag could be identified, false otherwise.</returns>
        public abstract bool IdentifyTag(in StringBuilder _input, int _startIndex, int _endIndex);

        /// <summary>
        /// Tries to parse a specific <see cref="string"/> value.
        /// </summary>
        /// <param name="_input"><see cref="StringBuilder"/> content.</param>
        /// <param name="_startIndex">Content start index.</param>
        /// <param name="_endIndex">Content end index.</param>
        /// <param name="_tag">Begin tag content.</param>
        /// <returns>True if the value could be parsed, false otherwise.</returns>
        public abstract bool Parse(StringBuilder _input, int _startIndex, int _endIndex, StringBuilder _tag);
        #endregion
    }

    /// <summary>
    /// Utility 
    /// </summary>
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public static class EnhancedStringParserUtility {
        #region Markup
        private struct Markup {
            public EnhancedStringParser Parser;
            public int StartTagIndex;
            public int StopTagIndex;

            // -------------------------------------------
            // Constructor(s)
            // -------------------------------------------

            public Markup(EnhancedStringParser _parser, int _startTagIndex, int _stopTagIndex) {
                StartTagIndex = _startTagIndex;
                StopTagIndex  = _stopTagIndex;
                Parser        = _parser;
            }
        }
        #endregion

        #region Initialization
        private static List<EnhancedStringParser> allParsers = new List<EnhancedStringParser>();

        // -------------------------------------------
        // Initialization
        // -------------------------------------------

        // Called after the first scene Awake.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize() {

            Type _parserType = typeof(EnhancedStringParser);

            Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var _assembly in _assemblies) {
                try {
                    Type[] _types = _assembly.GetTypes();

                    foreach (Type _type in _types) {

                        // Register.
                        if (!_type.IsAbstract && _type.IsSubclassOf(_parserType)) {
                            allParsers.Add(Activator.CreateInstance(_type) as EnhancedStringParser);
                        }
                    }
                } catch { }
            }
        }

        #if UNITY_EDITOR
        // Editor constructor.
        static EnhancedStringParserUtility() {
            Initialize();
        }
        #endif
        #endregion

        #region Parser
        private const char StartMakerChar   = '<';
        private const char StopMakerChar    = '>';
        private const char EndMakerChar     = '/';

        private static readonly StringBuilder contentBuilder = new StringBuilder();
        private static readonly StringBuilder tagBuilder     = new StringBuilder();

        private static readonly List<Markup> markups  = new List<Markup>();

        // -----------------------

        /// <returns>Parsed <see cref="string"/> value.</returns>
        /// <inheritdoc cref="ParseRef(ref string)"/>
        public static string Parse(string _input) {
            ParseRef(ref _input);
            return _input;
        }

        /// <summary>
        /// Parses a given input <see cref="string"/> value.
        /// </summary>
        /// <param name="_input">Input <see cref="string"/> value to parse.</param>
        public static void ParseRef(ref string _input) {

            StringBuilder _builder = contentBuilder;
            List<Markup> _markups = markups;

            // Reset data.
            ResetCache(_builder, _markups);

            int _startTagIndex = -1;
            int _stopTagIndex  = -1;
            bool _isClosure = false;

            _builder.Append(_input);

            for (int i = 0; i < _builder.Length; i++) {

                char _char = _builder[i];

                switch (_char) {

                    // Start.
                    case StartMakerChar:

                        // End on next.
                        int nextIndex = i + 1;

                        _isClosure = (nextIndex < _builder.Length) && (_builder[nextIndex] == EndMakerChar);
                        _startTagIndex = i;

                        break;

                    // Stop.
                    case StopMakerChar:

                        int _increment = _isClosure ? 1 : 0;
                        _stopTagIndex = i;

                        // Find tag parser.
                        if (GetParser(_builder, _startTagIndex + 1 + _increment, _stopTagIndex - 1, out EnhancedStringParser _parser)) {

                            //Debug.LogError("Parser => " + _parser.GetType() + " - " + _isClosure);

                            if (_isClosure) {

                                // Parse content.
                                DoParse(_builder, _startTagIndex, _stopTagIndex, _markups, _parser);

                            } else {

                                // Store parser.
                                _markups.Add(new Markup(_parser, _startTagIndex, _stopTagIndex));
                            }
                        } else {
                            //Debug.LogError("No Parser => " + allParsers.Count + " - " + _isClosure);
                        }

                        // Reset.
                        _startTagIndex = -1;
                        _stopTagIndex  = -1;
                        _isClosure = false;

                        break;

                    default:
                        break;
                }
            }

            // Update.
            _input = _builder.ToString();

            // Clear data.
            ResetCache(_builder, _markups);

            // ----- Local Methods ----- \\

            static void ResetCache(StringBuilder _builder, List<Markup> _markups) {
                _builder.Clear();
                _markups.Clear();
            }

            static bool DoParse(StringBuilder _builder, int _startIndex, int _stopIndex, List<Markup> _markups, EnhancedStringParser _parser) {

                StringBuilder _tagBuilder = tagBuilder;
                _tagBuilder.Clear();

                // Find last associated markup.
                for (int k = _markups.Count; k-- > 0;) {

                    Markup _markup = _markups[k];
                    if (_markup.Parser != _parser) {
                        continue;
                    }

                    // Parse.
                    int beginLength = (_markup.StopTagIndex - _markup.StartTagIndex) + 1;

                    for (int i = _markup.StartTagIndex + 1; i < _markup.StopTagIndex; i++) {
                        _tagBuilder.Append(_builder[i]);
                    }

                    int _endLength = (_stopIndex - _startIndex) + 1;

                    _builder.Remove(_startIndex, _endLength);               // Remove end tag.
                    _builder.Remove(_markup.StartTagIndex, beginLength);    // Remove begin tag.

                    _parser.Parse(_builder, _markup.StartTagIndex, _startIndex - (beginLength + 1), _tagBuilder);
                    _markups.RemoveAt(k);

                    return true;
                }

                return false;
            }
        }

        // -----------------------

        /// <summary>
        /// Get the <see cref="EnhancedStringParser"/> for a specific tag.
        /// </summary>
        /// <param name="_builder"><see cref="StringBuilder"/> content.</param>
        /// <param name="_startIndex">Tag start index.</param>
        /// <param name="_stopIndex">Tag stop index.</param>
        /// <param name="_parser"><see cref="EnhancedStringParser"/> associated with this tag (null if none).</param>
        /// <returns>True if a <see cref="EnhancedStringParser"/> coudl be found, false otherwise.</returns>
        public static bool GetParser(in StringBuilder _builder, int _startIndex, int _stopIndex, out EnhancedStringParser _parser) {

            // Empty.
            if ((_stopIndex - _startIndex) <= 0) {
                _parser = null;
                return false;
            }

            // Find parser.
            ref List<EnhancedStringParser> _span = ref allParsers;
            int _count = _span.Count;

            for (int i = 0; i < _count; i++) {

                _parser = _span[i];
                if (_parser.IdentifyTag(_builder, _startIndex, _stopIndex)) {
                    return true;
                }
            }

            _parser = null;
            return false;
        }
        #endregion
    }
}
