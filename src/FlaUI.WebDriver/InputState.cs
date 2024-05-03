namespace FlaUI.WebDriver
{
    public class InputState
    {
        private readonly Dictionary<string, InputSource> _inputStateMap = new();

        public List<Action> InputCancelList = new List<Action>();

        public void Reset()
        {
            InputCancelList.Clear();
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
                "key" => new KeyInputSource(),
                "pen" => throw new NotImplementedException("Pen input source is not implemented yet"),
                "touch" => throw new NotImplementedException("Touch input source is not implemented yet"),
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
        /// Note: The spec does not specify that a created input source should be added to the input state map.
        /// </remarks>
        public InputSource GetOrCreateInputSource(string type, string id)
        {
            var source = GetInputSource(id);

            if (source != null && source.Type != type)
            {
                throw WebDriverResponseException.InvalidArgument(
                    $"Input source with id '{id}' already exists and has a different type: {source.Type}");
            }

            return CreateInputSource(type);
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
        public void RemoveInputSource(string inputId) => _inputStateMap.Remove(inputId);
    }
}
