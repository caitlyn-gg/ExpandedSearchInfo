using System;
using ExpandedSearchInfo.Providers;

namespace ExpandedSearchInfo.Sections {
    public interface ISearchInfoSection {
        IProvider Provider { get; }
        string Name { get; }
        Uri Uri { get; }

        void Draw();
    }
}
