using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class FunkyLightingLogFilter {
    class Handler : ILogHandler {
        readonly ILogHandler inner;

        public Handler(ILogHandler inner) {
            this.inner = inner;
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args) {
            if (format != null && format.Contains("R8_SRGB' is not supported")) {
                return;
            }

            inner.LogFormat(logType, context, format, args);
        }

        public void LogException(System.Exception exception, Object context) {
            inner.LogException(exception, context);
        }
    }

    static FunkyLightingLogFilter() {
        if (!(Debug.unityLogger.logHandler is Handler)) {
            Debug.unityLogger.logHandler = new Handler(Debug.unityLogger.logHandler);
        }
    }
}
