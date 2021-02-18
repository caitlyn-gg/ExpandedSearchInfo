using System;

namespace ExpandedSearchInfo.Sections {
    public interface ISearchInfoSection {
        string Name { get; }
        Uri Uri { get; }

        void Draw();
    }
}
