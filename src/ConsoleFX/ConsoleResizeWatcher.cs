using System;

namespace ConsoleFX
    {
        public static class ConsoleResizeWatcher
            {
                private static bool _initialized = false;
                private static short _lastCols;
                private static short _lastRows;

                public static bool Initialize()
                    {
                        if (_initialized)
                            {
                                return true;
                            }

                        if (Console.IsOutputRedirected)
                            {
                                return false;
                            }

                        _lastCols = SafeWindowWidth();
                        _lastRows = SafeWindowHeight();
                        _initialized = true;
                        return true;
                    }

                public static void Restore()
                    {
                        _initialized = false;
                    }

                public static bool TryReadResizeEvent(int timeoutMs, out short cols, out short rows)
                    {
                        cols = 0;
                        rows = 0;

                        if (!_initialized && !Initialize())
                            {
                                return false;
                            }

                        short currentCols = SafeWindowWidth();
                        short currentRows = SafeWindowHeight();
                        if (currentCols != _lastCols || currentRows != _lastRows)
                            {
                                _lastCols = currentCols;
                                _lastRows = currentRows;
                                cols = currentCols;
                                rows = currentRows;
                                return true;
                            }

                        return false;
                    }

                private static short SafeWindowWidth()
                    {
                        return (short)Math.Max(Console.WindowWidth, 0);
                    }

                private static short SafeWindowHeight()
                    {
                        return (short)Math.Max(Console.WindowHeight, 0);
                    }
            }
    }