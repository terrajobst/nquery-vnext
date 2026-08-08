using System.Collections.Immutable;

using NQuery.Metadata;

namespace NQuery.Tests.Metadata;

public sealed class RelationshipDefinitionTests
{
    [Fact]
    public void Create_ThrowsForDefaultParentColumns()
    {
        var table = TableDefinition.Create("T", Array.Empty<Row>());

        var exception = Assert.Throws<ArgumentNullException>(() =>
            RelationshipDefinition.Create(table, default, table, table.Columns));
        Assert.Equal("parentColumns", exception.ParamName);
    }

    [Fact]
    public void Create_ThrowsForDefaultChildColumns()
    {
        var table = TableDefinition.Create("T", Array.Empty<Row>());

        var exception = Assert.Throws<ArgumentNullException>(() =>
            RelationshipDefinition.Create(table, table.Columns, table, default));
        Assert.Equal("childColumns", exception.ParamName);
    }

    private sealed class Row
    {
        public Row(int id) => Id = id;

        public int Id { get; }
    }
}
