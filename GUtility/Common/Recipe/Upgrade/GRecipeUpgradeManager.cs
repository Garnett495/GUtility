using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Common.Recipe.Core;
using GUtility.Common.Recipe.Exceptions;

namespace GUtility.Common.Recipe.Upgrade
{
    /// <summary>
    /// Recipe 升版管理器。
    /// </summary>
    /// <typeparam name="T">Recipe 資料型別。</typeparam>
    public class GRecipeUpgradeManager<T> where T : class, new()
    {
        private readonly List<IGRecipeUpgrader<T>> _upgraders;

        /// <summary>
        /// 建構子。
        /// </summary>
        public GRecipeUpgradeManager()
        {
            _upgraders = new List<IGRecipeUpgrader<T>>();
        }

        /// <summary>
        /// 註冊升版器。
        /// </summary>
        /// <param name="upgrader">升版器。</param>
        public void Register(IGRecipeUpgrader<T> upgrader)
        {
            if (upgrader == null)
                throw new ArgumentNullException("upgrader");

            int i;
            for (i = 0; i < _upgraders.Count; i++)
            {
                if (_upgraders[i].SourceVersion == upgrader.SourceVersion)
                {
                    throw new InvalidOperationException(
                        string.Format("Upgrader for source version {0} is already registered.", upgrader.SourceVersion));
                }
            }

            _upgraders.Add(upgrader);
            _upgraders.Sort(CompareUpgraderBySourceVersion);
        }

        /// <summary>
        /// 是否存在可用的升版器。
        /// </summary>
        /// <param name="sourceVersion">來源版本。</param>
        /// <returns>存在回傳 true，否則 false。</returns>
        public bool HasUpgrader(int sourceVersion)
        {
            return FindUpgrader(sourceVersion) != null;
        }

        /// <summary>
        /// 取得目前已註冊的最高目標版本。
        /// </summary>
        public int GetLatestVersion()
        {
            if (_upgraders.Count == 0)
                return 1;

            int latest = 1;
            int i;
            for (i = 0; i < _upgraders.Count; i++)
            {
                if (_upgraders[i].TargetVersion > latest)
                    latest = _upgraders[i].TargetVersion;
            }

            return latest;
        }

        /// <summary>
        /// 將 Recipe 文件升級到最新版本。
        /// </summary>
        /// <param name="document">待升級文件。</param>
        /// <returns>升級後文件。</returns>
        public GRecipeDocument<T> UpgradeToLatest(GRecipeDocument<T> document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            int latestVersion = GetLatestVersion();
            return Upgrade(document, latestVersion);
        }

        /// <summary>
        /// 將 Recipe 文件升級到指定版本。
        /// </summary>
        /// <param name="document">待升級文件。</param>
        /// <param name="targetVersion">目標版本。</param>
        /// <returns>升級後文件。</returns>
        public GRecipeDocument<T> Upgrade(GRecipeDocument<T> document, int targetVersion)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            if (targetVersion <= 0)
                throw new ArgumentOutOfRangeException("targetVersion");

            if (document.SchemaVersion <= 0)
                document.SchemaVersion = 1;

            if (document.SchemaVersion > targetVersion)
            {
                throw new GRecipeUpgradeException(
                    string.Format(
                        "Document schema version {0} is newer than target version {1}.",
                        document.SchemaVersion,
                        targetVersion));
            }

            GRecipeDocument<T> current = document;

            while (current.SchemaVersion < targetVersion)
            {
                IGRecipeUpgrader<T> upgrader = FindUpgrader(current.SchemaVersion);
                if (upgrader == null)
                {
                    throw new GRecipeUpgradeException(
                        string.Format(
                            "No upgrader registered for schema version {0}.",
                            current.SchemaVersion));
                }

                if (upgrader.TargetVersion <= upgrader.SourceVersion)
                {
                    throw new GRecipeUpgradeException(
                        string.Format(
                            "Invalid upgrader version mapping: {0} -> {1}.",
                            upgrader.SourceVersion,
                            upgrader.TargetVersion));
                }

                try
                {
                    current = upgrader.Upgrade(current);

                    if (current == null)
                    {
                        throw new GRecipeUpgradeException(
                            string.Format(
                                "Upgrader returned null for schema version {0}.",
                                upgrader.SourceVersion));
                    }

                    current.SchemaVersion = upgrader.TargetVersion;
                    current.Touch();
                }
                catch (GRecipeUpgradeException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new GRecipeUpgradeException(
                        string.Format(
                            "Failed to upgrade recipe from schema version {0} to {1}.",
                            upgrader.SourceVersion,
                            upgrader.TargetVersion),
                        ex);
                }
            }

            return current;
        }

        private IGRecipeUpgrader<T> FindUpgrader(int sourceVersion)
        {
            int i;
            for (i = 0; i < _upgraders.Count; i++)
            {
                if (_upgraders[i].SourceVersion == sourceVersion)
                    return _upgraders[i];
            }

            return null;
        }

        private static int CompareUpgraderBySourceVersion(
            IGRecipeUpgrader<T> x,
            IGRecipeUpgrader<T> y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.SourceVersion.CompareTo(y.SourceVersion);
        }
    }
}
