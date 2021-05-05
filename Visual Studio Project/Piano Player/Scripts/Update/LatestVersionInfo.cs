using System;

namespace Piano_Player.Update
{
    [Serializable]
    public class LatestVersionInfo
    {
        // =======================================================
        [Serializable]
        public class PVersion
        {
            public static explicit operator Version(PVersion a)
            {
                return new Version(a.Major, a.Minor, a.Build, a.Revision);
            }

            public int Major { get; set; }
            public int Minor { get; set; }
            public int Build { get; set; }
            public int Revision { get; set; }

            public PVersion() { }
            public PVersion(int maj, int min, int bui, int rev)
            {
                Major = maj; Minor = min; Build = bui; Revision = rev;
            }

            public int CompareTo(Version v)
            {
                return ((Version)this).CompareTo(v);
            }
            public int CompareTo(PVersion v)
            {
                return ((Version)this).CompareTo(((Version)v));
            }
        }
        // =======================================================
        /// <summary>Latest available version (4 digits separated by full stops).</summary>
        public PVersion LatestVersion { get; set; }
        public string UpdateInstallerURL { get; set; }
        // ------------------------------------------------------
        // =======================================================
    }
}
