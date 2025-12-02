import DescriptionIcon from '@mui/icons-material/Description';
import StorageIcon from '@mui/icons-material/Storage';
import AutorenewIcon from '@mui/icons-material/Autorenew';
import ApiIcon from '@mui/icons-material/Api';
import CodeIcon from '@mui/icons-material/Code';
import DataObjectIcon from '@mui/icons-material/DataObject';
import JavascriptIcon from '@mui/icons-material/Javascript';
import WebIcon from '@mui/icons-material/Web';

/**
 * Maps file types to their corresponding Material-UI icons
 * @param type - The file type (entity, repository, handler, api, etc.)
 * @returns The appropriate Material-UI icon component
 */
export const getFileTypeIcon = (type: string) => {
  const normalizedType = type.toLowerCase();

  switch (normalizedType) {
    case 'entity':
      return <DescriptionIcon />;
    case 'repository':
      return <StorageIcon />;
    case 'handler':
    case 'command':
    case 'query':
      return <AutorenewIcon />;
    case 'api':
    case 'controller':
      return <ApiIcon />;
    case 'model':
    case 'dto':
      return <DataObjectIcon />;
    case 'typescript':
      return <JavascriptIcon />;
    case 'react':
      return <WebIcon />;
    default:
      return <CodeIcon />;
  }
};
/**
 * Gets a color for a file type (for consistent theming)
 * @param type - The file type
 * @returns A Material-UI color string
 */
export const getFileTypeColor = (type: string): string => {
  const normalizedType = type.toLowerCase();

  switch (normalizedType) {
    case 'entity':
      return 'primary';
    case 'repository':
      return 'secondary';
    case 'handler':
    case 'command':
    case 'query':
      return 'info';
    case 'api':
    case 'controller':
      return 'success';
    case 'model':
    case 'dto':
      return 'warning';
    case 'typescript':
      return 'info';
    case 'react':
      return 'primary';
    default:
      return 'default';
  }
};
