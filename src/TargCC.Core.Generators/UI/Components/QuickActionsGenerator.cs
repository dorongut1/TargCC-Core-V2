using System.Globalization;
using System.Text;
using TargCC.Core.Interfaces.Models;

namespace TargCC.Core.Generators.UI.Components;

/// <summary>
/// Generates quick action buttons for DataGrid rows.
/// </summary>
public static class QuickActionsGenerator
{
    /// <summary>
    /// Generates action column definition for DataGrid.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="entityName">Entity name (PascalCase).</param>
    /// <returns>GridColDef for actions column.</returns>
    public static string GenerateActionsColumn(Table table, string entityName)
    {
        var hasSoftDelete = table.Columns.Exists(c =>
            c.Name.Equals("DeletedOn", StringComparison.OrdinalIgnoreCase));

        var sb = new StringBuilder();

        sb.AppendLine("  {");
        sb.AppendLine("    field: 'actions',");
        sb.AppendLine("    headerName: 'Actions',");
        sb.AppendLine("    width: 150,");
        sb.AppendLine("    sortable: false,");
        sb.AppendLine("    filterable: false,");
        sb.AppendLine("    renderCell: (params) => (");
        sb.AppendLine("      <Box>");
        sb.AppendLine("        <IconButton");
        sb.AppendLine("          size=\"small\"");
        sb.AppendLine("          onClick={() => handleEdit(params.row.id)}");
        sb.AppendLine("          title=\"Edit\"");
        sb.AppendLine("        >");
        sb.AppendLine("          <EditIcon fontSize=\"small\" />");
        sb.AppendLine("        </IconButton>");
        sb.AppendLine("        <IconButton");
        sb.AppendLine("          size=\"small\"");
        sb.AppendLine("          onClick={() => handleView(params.row.id)}");
        sb.AppendLine("          title=\"View Details\"");
        sb.AppendLine("        >");
        sb.AppendLine("          <VisibilityIcon fontSize=\"small\" />");
        sb.AppendLine("        </IconButton>");
        sb.AppendLine("        <IconButton");
        sb.AppendLine("          size=\"small\"");
        sb.AppendLine("          onClick={() => handleDelete(params.row.id)}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"          title=\"{(hasSoftDelete ? "Archive" : "Delete")}\"");
        sb.AppendLine("        >");
        sb.AppendLine("          <DeleteIcon fontSize=\"small\" color=\"error\" />");
        sb.AppendLine("        </IconButton>");
        sb.AppendLine("      </Box>");
        sb.AppendLine("    )");
        sb.AppendLine("  }");

        return sb.ToString();
    }

    /// <summary>
    /// Generates action handler functions.
    /// </summary>
    /// <param name="entityName">Entity name (PascalCase).</param>
    /// <param name="hasSoftDelete">Whether table has soft delete.</param>
    /// <returns>Handler function code.</returns>
    public static string GenerateActionHandlers(string entityName, bool hasSoftDelete)
    {
        var sb = new StringBuilder();

        // Edit handler
        sb.AppendLine("  const handleEdit = (id: number) => {");
        sb.AppendLine("    navigate(`/edit/${id}`);");
        sb.AppendLine("  };");
        sb.AppendLine();

        // View handler
        sb.AppendLine("  const handleView = (id: number) => {");
        sb.AppendLine("    navigate(`/view/${id}`);");
        sb.AppendLine("  };");
        sb.AppendLine();

        // Delete handler with confirmation
        sb.AppendLine("  const handleDelete = async (id: number) => {");
        sb.AppendLine(CultureInfo.InvariantCulture, $"    if (!window.confirm('{(hasSoftDelete ? "Archive this item?" : "Permanently delete this item?")}')) {{");
        sb.AppendLine("      return;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    try {");
        sb.AppendLine("      await deleteMutation.mutateAsync(id);");
        sb.AppendLine("      // Refresh list");
        sb.AppendLine("      queryClient.invalidateQueries({ queryKey: ['list'] });");
        sb.AppendLine("    } catch (error) {");
        sb.AppendLine("      console.error('Delete failed:', error);");
        sb.AppendLine("      alert('Failed to delete item');");
        sb.AppendLine("    }");
        sb.AppendLine("  };");

        return sb.ToString();
    }

    /// <summary>
    /// Generates required imports for actions.
    /// </summary>
    /// <returns>Import statements.</returns>
    public static string GenerateActionImports()
    {
        var sb = new StringBuilder();

        sb.AppendLine("import { useNavigate } from 'react-router-dom';");
        sb.AppendLine("import { IconButton, Box } from '@mui/material';");
        sb.AppendLine("import EditIcon from '@mui/icons-material/Edit';");
        sb.AppendLine("import DeleteIcon from '@mui/icons-material/Delete';");
        sb.AppendLine("import VisibilityIcon from '@mui/icons-material/Visibility';");
        sb.AppendLine("import { useQueryClient } from '@tanstack/react-query';");

        return sb.ToString();
    }
}
