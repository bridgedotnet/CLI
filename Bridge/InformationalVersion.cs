using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.CLI
{
    public class InformationalVersion : IComparable<InformationalVersion>
    {
        private Version ver;

        public int Build { get { return ver.Build; } }

        public int Minor { get { return ver.Minor; } }

        public int Major { get { return ver.Major; } }

        public bool IsPrerelease { get { return !string.IsNullOrWhiteSpace(Suffix); } }

        public string Suffix { get; }

        public InformationalVersion(string version)
        {
            int major, minor, build;

            if (version.Length < 5)
            {
                throw new FormatException("Invalid version string: " + version);
            }

            var verFields = version.Split('.');

            if (verFields.Length != 3)
            {
                throw new FormatException("Invalid version string: " + version);
            }

            var versionSuffix = "";
            if (verFields[2].IndexOf('-') > 0)
            {
                versionSuffix = verFields[2].Substring(verFields[2].IndexOf('-') + 1);
                verFields[2] = verFields[2].Substring(0, verFields[2].IndexOf('-'));
            }

            if (!int.TryParse(verFields[0], out major))
            {
                throw new FormatException("Major version field can't be mapped to int: " + verFields[0]);
            }

            if (!int.TryParse(verFields[1], out minor))
            {
                throw new FormatException("Minor version field can't be mapped to int: " + verFields[1]);
            }

            if (!int.TryParse(verFields[2], out build))
            {
                throw new FormatException("'Build' version field can't be mapped to int: " + verFields[2]);
            }

            ver = new Version(major, minor, build);

            Suffix = versionSuffix;
        }

        public InformationalVersion(int major, int minor = 0, int build = 0, string suffix = "")
        {
            ver = new Version(major, minor, build);
            Suffix = suffix;
        }

        public int CompareTo(string next)
        {
            return CompareTo(new InformationalVersion(next));
        }

        public int CompareTo(InformationalVersion next)
        {
            if (this > next)
            {
                return 1;
            }
            else if (this < next)
            {
                return -1;
            }

            return 0;
        }

        public static implicit operator Version(InformationalVersion version)
        {
            return version.ver;
        }

        public bool Equals(InformationalVersion version)
        {
            return version != null && ((Version)this).Equals(version) && Suffix == version.Suffix;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(InformationalVersion current, InformationalVersion next)
        {
            return (current is null && next is null) || !(current is null) && current.Equals(next);
        }

        public static bool operator !=(InformationalVersion current, InformationalVersion next)
        {
            return !(current == next);
        }

        public static bool operator <(InformationalVersion current, InformationalVersion next)
        {
            if (current.Equals(next))
            {
                return false;
            }
            else if ((Version)current < next)
            {
                return true;
            }
            else
            {
                if ((Version)current == next)
                {
                    if (next.Suffix.Length > 0)
                    {
                        if (current.Suffix.Length == 0)
                        {
                            return false;
                        }
                        else
                        {
                            return current.Suffix.CompareTo(next.Suffix) < 0;
                        }
                    }
                    else
                    {
                        return !(current.Suffix.Length > 0);
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool operator >(InformationalVersion current, InformationalVersion next)
        {
            return !(current.Equals(next) || current < next);
        }

        public static bool operator <=(InformationalVersion current, InformationalVersion next)
        {
            return current.Equals(next) || current < next;
        }

        public static bool operator >=(InformationalVersion current, InformationalVersion next)
        {
            return current.Equals(next) || current > next;
        }

        public override string ToString()
        {
            return ver.Major + "." + ver.Minor + "." + ver.Build + (string.IsNullOrWhiteSpace(Suffix) ? "" : "-" + Suffix);
        }
    }
}
