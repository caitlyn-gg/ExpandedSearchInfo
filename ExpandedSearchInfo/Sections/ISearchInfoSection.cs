using ExpandedSearchInfo.Providers;
using System;

namespace ExpandedSearchInfo.Sections;

public interface ISearchInfoSection {
    IProvider Provider { get; }
    string Name { get; }
    Uri Uri { get; }

    void Draw();
}