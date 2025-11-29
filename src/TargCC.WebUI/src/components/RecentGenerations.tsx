import { Card, CardContent, CardHeader, List, ListItem, ListItemText, ListItemIcon, Chip, Box, Typography } from '@mui/material';
import { CheckCircle, Error, Code, Storage, Api, TableChart } from '@mui/icons-material';

interface RecentGeneration {
  id: string;
  tableName: string;
  type: 'Entity' | 'Repository' | 'API' | 'SQL';
  timestamp: Date;
  status: 'Success' | 'Failed';
}

interface RecentGenerationsProps {
  generations?: RecentGeneration[];
  maxItems?: number;
}

const RecentGenerations = ({ generations, maxItems = 5 }: RecentGenerationsProps) => {
  // Mock data if none provided
  const mockGenerations: RecentGeneration[] = [
    {
      id: '1',
      tableName: 'Customer',
      type: 'Entity',
      timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000), // 2 hours ago
      status: 'Success'
    },
    {
      id: '2',
      tableName: 'Order',
      type: 'Repository',
      timestamp: new Date(Date.now() - 4 * 60 * 60 * 1000), // 4 hours ago
      status: 'Success'
    },
    {
      id: '3',
      tableName: 'Product',
      type: 'API',
      timestamp: new Date(Date.now() - 6 * 60 * 60 * 1000), // 6 hours ago
      status: 'Failed'
    },
    {
      id: '4',
      tableName: 'Invoice',
      type: 'SQL',
      timestamp: new Date(Date.now() - 8 * 60 * 60 * 1000), // 8 hours ago
      status: 'Success'
    },
    {
      id: '5',
      tableName: 'Employee',
      type: 'Entity',
      timestamp: new Date(Date.now() - 24 * 60 * 60 * 1000), // 1 day ago
      status: 'Success'
    }
  ];

  const displayGenerations = (generations || mockGenerations).slice(0, maxItems);

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'Entity':
        return <Code />;
      case 'Repository':
        return <Storage />;
      case 'API':
        return <Api />;
      case 'SQL':
        return <TableChart />;
      default:
        return <Code />;
    }
  };

  const getStatusIcon = (status: string) => {
    return status === 'Success' ? 
      <CheckCircle color="success" /> : 
      <Error color="error" />;
  };

  const formatTimeAgo = (date: Date) => {
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
    
    if (seconds < 60) return 'just now';
    if (seconds < 3600) return `${Math.floor(seconds / 60)} minutes ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)} hours ago`;
    if (seconds < 604800) return `${Math.floor(seconds / 86400)} days ago`;
    return date.toLocaleDateString();
  };

  return (
    <Card>
      <CardHeader 
        title="Recent Generations"
        titleTypographyProps={{ variant: 'h6' }}
      />
      <CardContent sx={{ pt: 0 }}>
        {displayGenerations.length === 0 ? (
          <Typography color="text.secondary" align="center" py={3}>
            No recent generations
          </Typography>
        ) : (
          <List disablePadding>
            {displayGenerations.map((gen, index) => (
              <ListItem 
                key={gen.id}
                divider={index < displayGenerations.length - 1}
                sx={{ 
                  px: 0,
                  '&:hover': { 
                    bgcolor: 'action.hover',
                    cursor: 'pointer'
                  }
                }}
              >
                <ListItemIcon sx={{ minWidth: 40 }}>
                  {getStatusIcon(gen.status)}
                </ListItemIcon>
                <ListItemText
                  primary={
                    <Box display="flex" alignItems="center" gap={1}>
                      <Typography variant="body1" component="span">
                        {gen.tableName}
                      </Typography>
                      <Chip 
                        icon={getTypeIcon(gen.type)}
                        label={gen.type}
                        size="small"
                        variant="outlined"
                      />
                    </Box>
                  }
                  secondary={formatTimeAgo(gen.timestamp)}
                />
              </ListItem>
            ))}
          </List>
        )}
      </CardContent>
    </Card>
  );
};

export default RecentGenerations;
