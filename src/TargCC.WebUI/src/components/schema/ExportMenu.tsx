import { useState } from 'react';
import {
  IconButton,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Tooltip,
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import CodeIcon from '@mui/icons-material/Code';
import StorageIcon from '@mui/icons-material/Storage';
import DescriptionIcon from '@mui/icons-material/Description';
import type { DatabaseSchema } from '../../types/schema';
import { exportAsJSON, exportAsSQL, exportAsMarkdown } from '../../utils/schemaExport';
import { downloadFile } from '../../utils/downloadCode';

/**
 * Props for ExportMenu component
 */
interface ExportMenuProps {
  /** Database schema to export */
  schema: DatabaseSchema;
}

/**
 * Export Menu Component
 * Provides dropdown menu for exporting schema in various formats
 */
export default function ExportMenu({ schema }: ExportMenuProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  /**
   * Handle menu open
   */
  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  /**
   * Handle menu close
   */
  const handleClose = () => {
    setAnchorEl(null);
  };

  /**
   * Handle export in specified format
   */
  const handleExport = (format: 'json' | 'sql' | 'md') => {
    let content: string;
    let filename: string;

    switch (format) {
      case 'json':
        content = exportAsJSON(schema);
        filename = 'database-schema.json';
        break;
      case 'sql':
        content = exportAsSQL(schema);
        filename = 'database-schema.sql';
        break;
      case 'md':
        content = exportAsMarkdown(schema);
        filename = 'database-schema.md';
        break;
    }

    downloadFile(filename, content);
    handleClose();
  };

  return (
    <>
      <Tooltip title="Export Schema">
        <IconButton
          onClick={handleClick}
          aria-label="export schema"
          aria-controls={open ? 'export-menu' : undefined}
          aria-haspopup="true"
          aria-expanded={open ? 'true' : undefined}
        >
          <DownloadIcon />
        </IconButton>
      </Tooltip>

      <Menu
        id="export-menu"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        MenuListProps={{
          'aria-labelledby': 'export-button',
        }}
      >
        <MenuItem onClick={() => handleExport('json')}>
          <ListItemIcon>
            <CodeIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText
            primary="Export as JSON"
            secondary="Structured data format"
          />
        </MenuItem>

        <MenuItem onClick={() => handleExport('sql')}>
          <ListItemIcon>
            <StorageIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText primary="Export as SQL" secondary="DDL CREATE statements" />
        </MenuItem>

        <MenuItem onClick={() => handleExport('md')}>
          <ListItemIcon>
            <DescriptionIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText
            primary="Export as Markdown"
            secondary="Documentation format"
          />
        </MenuItem>
      </Menu>
    </>
  );
}
