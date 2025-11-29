/**
 * Sidebar Component
 * Navigation menu for the application
 */

import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Divider,
  Tooltip,
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import TableChartIcon from '@mui/icons-material/TableChart';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import SecurityIcon from '@mui/icons-material/Security';
import CodeIcon from '@mui/icons-material/Code';
import ChatIcon from '@mui/icons-material/Chat';
import { useNavigate, useLocation } from 'react-router-dom';

interface SidebarProps {
  open: boolean;
  width: number;
}

const menuItems = [
  { text: 'Dashboard', icon: <DashboardIcon />, path: '/' },
  { text: 'Tables', icon: <TableChartIcon />, path: '/tables' },
  { text: 'Generate', icon: <CodeIcon />, path: '/generate' },
  { text: 'AI Suggestions', icon: <AutoAwesomeIcon />, path: '/suggestions' },
  { text: 'Security', icon: <SecurityIcon />, path: '/security' },
  { text: 'AI Chat', icon: <ChatIcon />, path: '/chat' },
];

export const Sidebar: React.FC<SidebarProps> = ({ open, width }) => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleNavigation = (path: string) => {
    navigate(path);
  };

  return (
    <Drawer
      variant="persistent"
      open={open}
      sx={{
        width: width,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: width,
          boxSizing: 'border-box',
        },
      }}
    >
      <Toolbar />
      <Divider />
      <List>
        {menuItems.map((item) => (
          <ListItem key={item.text} disablePadding>
            <Tooltip title={item.text} placement="right" arrow>
              <ListItemButton
                selected={location.pathname === item.path}
                onClick={() => handleNavigation(item.path)}
                sx={{
                  '&.Mui-selected': {
                    backgroundColor: 'primary.light',
                    '&:hover': {
                      backgroundColor: 'primary.light',
                    },
                  },
                }}
              >
                <ListItemIcon 
                  sx={{ 
                    color: location.pathname === item.path ? 'primary.main' : 'inherit' 
                  }}
                >
                  {item.icon}
                </ListItemIcon>
                <ListItemText primary={item.text} />
              </ListItemButton>
            </Tooltip>
          </ListItem>
        ))}
      </List>
    </Drawer>
  );
};
