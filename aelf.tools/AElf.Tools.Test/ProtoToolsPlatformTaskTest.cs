#region Copyright notice and license

#endregion

// For NETCORE tests.
using Microsoft.Build.Framework;
using Moq;
using NUnit.Framework;

namespace AElf.Tools.Test
{
    public class ProtoToolsPlatformTaskTest
    {
        ProtoToolsPlatform _task;
        int _cpuMatched, _osMatched;

        [OneTimeSetUp]
        public void SetUp()
        {
            var mockEng = new Mock<IBuildEngine>();
            _task = new ProtoToolsPlatform() { BuildEngine = mockEng.Object };
            _task.Execute();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Assert.AreEqual(1, _cpuMatched, "CPU type detection failed");
            Assert.AreEqual(1, _osMatched, "OS detection failed");
        }

#if NETCORE
        // PlatformAttribute not yet available in NUnit, coming soon:
        // https://github.com/nunit/nunit/pull/3003.
        // Use same test case names as under the full framework.
        [Test]
        public void CpuIsX86()
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X86)
            {
                _cpuMatched++;
                Assert.AreEqual("x86", _task.Cpu);
            }
        }

        [Test]
        public void CpuIsX64()
        {
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                _cpuMatched++;
                Assert.AreEqual("x64", _task.Cpu);
            }
        }

        [Test]
        public void CpuIsArm64()
        {
            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                _cpuMatched++;

                // On macosx arm64, x64 is used until a native protoc is shipped
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Assert.AreEqual("x64", _task.Cpu);
                }
                // On windows arm64, x86 is used until a native protoc is shipped
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Assert.AreEqual("x86", _task.Cpu);
                }
                else
                {
                    Assert.AreEqual("arm64", _task.Cpu);
                }
            }
        }

        [Test]
        public void OsIsWindows()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _osMatched++;
                Assert.AreEqual("windows", _task.Os);
            }
        }

        [Test]
        public void OsIsLinux()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _osMatched++;
                Assert.AreEqual("linux", _task.Os);
            }
        }

        [Test]
        public void OsIsMacOsX()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _osMatched++;
                Assert.AreEqual("macosx", _task.Os);
            }
        }

#else  // !NETCORE, i.e. full framework.

        [Test, Platform("32-Bit")]
        public void CpuIsX86()
        {
            _cpuMatched++;
            Assert.AreEqual("x86", _task.Cpu);
        }

        [Test, Platform("64-Bit")]
        public void CpuIsX64()
        {
            _cpuMatched++;
            Assert.AreEqual("x64", _task.Cpu);
        }

        [Test, Platform("Win")]
        public void OsIsWindows()
        {
            _osMatched++;
            Assert.AreEqual("windows", _task.Os);
        }

        [Test, Platform("Linux")]
        public void OsIsLinux()
        {
            _osMatched++;
            Assert.AreEqual("linux", _task.Os);
        }

        [Test, Platform("MacOSX")]
        public void OsIsMacOsX()
        {
            _osMatched++;
            Assert.AreEqual("macosx", _task.Os);
        }

#endif  // NETCORE
    };
}
