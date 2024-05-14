using System.Diagnostics;

namespace FlaUI.WebDriver
{
    public class InputState
    {
        private readonly Dictionary<string, InputSource> _inputStateMap = new();

        public List<Action> InputCancelList = new List<Action>();

        public void Reset()
        {
            InputCancelList.Clear();
            _inputStateMap.Clear();
        }

        /// <summary>
        /// Creates an input source of the given type.
        /// </summary>
        /// <remarks>
        /// Implements "create an input source" from https://www.w3.org/TR/webdriver2/#input-state
        /// Note: The spec does not specify that a created input source should be added to the input state map.
        /// </remarks>
        public InputSource CreateInputSource(string type)
        {
            return type switch
            {
                "none" => throw new NotImplementedException("Null input source is not implemented yet"),
                "key" => new KeyInputSource(),
                "pointer" => throw new NotImplementedException("Pointer input source is not implemented yet"),
                "wheel" => throw new NotImplementedException("Wheel input source is not implemented yet"),
                _ => throw new InvalidOperationException($"Unknown input source type: {type}")
            };
        }

        /// <summary>
        /// Tries to get an input source with the specified input ID.
        /// </summary>
        /// <remarks>
        /// Implements "get an input source" from https://www.w3.org/TR/webdriver2/#input-state
        /// </remarks>
        public InputSource? GetInputSource(string inputId)
        {
            _inputStateMap.TryGetValue(inputId, out var result);
            return result;
        }

        /// <summary>
        /// Tries to get an input source with the specified input ID.
        /// </summary>
        /// <remarks>
        /// Implements "get an input source" from https://www.w3.org/TR/webdriver2/#input-state
        /// </remarks>
        public T? GetInputSource<T>(string inputId) where T : InputSource
        {
            if (GetInputSource(inputId) is { } source)
            {
                if (source is T result)
                {
                    return result;
                }
                else
                {
                    throw WebDriverResponseException.InvalidArgument(
                        $"Input source with id '{inputId}' is not of the expected type: {typeof(T).Name}");
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an input source or creates a new one if it does not exist.
        /// </summary>
        /// <remarks>
        /// Implements "get or create an input source" from https://www.w3.org/TR/webdriver2/#input-state
        /// Note: The spec does not specify that a created input source should be added to the input state map
        /// but this implementation does.
        /// </remarks>
        public InputSource GetOrCreateInputSource(string type, string id)
        {
            var source = GetInputSource(id);

            if (source != null && source.Type != type)
            {
                throw WebDriverResponseException.InvalidArgument(
                    $"Input source with id '{id}' already exists and has a different type: {source.Type}");
            }

            // Note: The spec does not specify that a created input source should be added to the input state map,
            // however it needs to be added somewhere. The caller can't do it because it doesn't know if the source
            // was created or already existed. See https://github.com/w3c/webdriver/issues/1810
            if (source == null)
            {
                source = CreateInputSource(type);
                AddInputSource(id, source);
            }

            return source;
        }

        /// <summary>
        /// Adds an input source.
        /// </summary>
        /// <remarks>
        /// Implements "add an input source" from https://www.w3.org/TR/webdriver2/#input-state
        /// </remarks>
        public void AddInputSource(string inputId, InputSource inputSource) => _inputStateMap.Add(inputId, inputSource);

        /// <summary>
        /// Removes an input source.
        /// </summary>
        /// <remarks>
        /// Implements "remove an input source" from https://www.w3.org/TR/webdriver2/#input-state
        /// </remarks>
        public void RemoveInputSource(string inputId)
        {
            Debug.Assert(!InputCancelList.Any(x => x.Id == inputId));

            _inputStateMap.Remove(inputId);
        }
    }
}
