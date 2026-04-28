/*using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using ChatModule;
using Xunit;

namespace ChatModule.Tests
{
    public class AppTests
    {
        [Fact]
        public void BuildCrashEntry_ContainsSource()
        {
            var entry = App.BuildCrashEntry("UnitTestSource", "details", new DateTime(2026, 4, 17, 10, 0, 0));

            Assert.Contains("UnitTestSource", entry);
        }

        [Fact]
        public void TryAppendCrashLog_ValidDirectory_ReturnsTrue()
        {
            var directory = Path.Combine(Path.GetTempPath(), "ChatModuleTests", Guid.NewGuid().ToString());

            var result = App.TryAppendCrashLog(directory, "Src", "Details");

            Assert.True(result);
        }

        [Fact]
        public void TryAppendCrashLog_InvalidDirectory_ReturnsFalse()
        {
            var result = App.TryAppendCrashLog("\0", "Src", "Details");

            Assert.False(result);
        }

        [Fact]
        public void SetMainWindow_Null_AssignsNull()
        {
            App.SetMainWindow(null!);

            Assert.Null(App.MainAppWindow);
        }

        [Fact]
        public void DatabaseManager_Getter_DefaultIsNull()
        {
            var app = (App)RuntimeHelpers.GetUninitializedObject(typeof(App));

            Assert.Null(app.DatabaseManager);
        }

        [Fact]
        public void LogException_CreatesCrashLogFile()
        {
            var method = typeof(App).GetMethod("LogException", BindingFlags.NonPublic | BindingFlags.Static)!;
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChatModule", "crash.log");

            method.Invoke(null, new object[] { "UnitSource", "UnitDetails" });

            Assert.True(File.Exists(logPath));
        }
    }
}
*/