import { Card, CardContent, CardHeader, Typography, Box } from '@mui/material';
import { Timeline, TimelineItem, TimelineSeparator, TimelineConnector, TimelineContent, TimelineDot, TimelineOppositeContent } from '@mui/lab';
import { Code, Security, Analytics, Refresh } from '@mui/icons-material';

interface Activity {
  id: string;
  type: 'Generation' | 'Scan' | 'Analysis' | 'Refresh';
  description: string;
  timestamp: Date;
  user: string;
}

interface ActivityTimelineProps {
  activities?: Activity[];
  maxItems?: number;
}

const ActivityTimeline = ({ activities, maxItems = 10 }: ActivityTimelineProps) => {
  // Mock data if none provided
  const mockActivities: Activity[] = [
    {
      id: '1',
      type: 'Generation',
      description: 'Generated entities for Customer table',
      timestamp: new Date(Date.now() - 30 * 60 * 1000), // 30 min ago
      user: 'System'
    },
    {
      id: '2',
      type: 'Scan',
      description: 'Security scan completed - 0 issues found',
      timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000), // 2 hours ago
      user: 'Admin'
    },
    {
      id: '3',
      type: 'Analysis',
      description: 'Code quality analysis for Order module',
      timestamp: new Date(Date.now() - 4 * 60 * 60 * 1000), // 4 hours ago
      user: 'Developer'
    },
    {
      id: '4',
      type: 'Refresh',
      description: 'Schema refreshed from database',
      timestamp: new Date(Date.now() - 6 * 60 * 60 * 1000), // 6 hours ago
      user: 'System'
    },
    {
      id: '5',
      type: 'Generation',
      description: 'Generated SQL procedures for Product table',
      timestamp: new Date(Date.now() - 8 * 60 * 60 * 1000), // 8 hours ago
      user: 'System'
    }
  ];

  const displayActivities = (activities || mockActivities).slice(0, maxItems);

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'Generation':
        return <Code />;
      case 'Scan':
        return <Security />;
      case 'Analysis':
        return <Analytics />;
      case 'Refresh':
        return <Refresh />;
      default:
        return <Code />;
    }
  };

  const getActivityColor = (type: string): 'primary' | 'success' | 'warning' | 'info' => {
    switch (type) {
      case 'Generation':
        return 'primary';
      case 'Scan':
        return 'success';
      case 'Analysis':
        return 'warning';
      case 'Refresh':
        return 'info';
      default:
        return 'primary';
    }
  };

  const formatTime = (date: Date) => {
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  };

  const formatTimeAgo = (date: Date) => {
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
    
    if (seconds < 60) return 'just now';
    if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
    return `${Math.floor(seconds / 86400)}d ago`;
  };

  return (
    <Card>
      <CardHeader 
        title="Activity Timeline"
        titleTypographyProps={{ variant: 'h6' }}
      />
      <CardContent sx={{ pt: 0 }}>
        {displayActivities.length === 0 ? (
          <Typography color="text.secondary" align="center" py={3}>
            No recent activity
          </Typography>
        ) : (
          <Timeline 
            sx={{ 
              m: 0, 
              p: 0,
              '& .MuiTimelineItem-root:before': {
                flex: 0,
                padding: 0
              }
            }}
          >
            {displayActivities.map((activity, index) => (
              <TimelineItem key={activity.id}>
                <TimelineOppositeContent 
                  sx={{ 
                    m: 0, 
                    p: 0, 
                    maxWidth: '80px',
                    pr: 1
                  }}
                >
                  <Typography variant="caption" color="text.secondary">
                    {formatTimeAgo(activity.timestamp)}
                  </Typography>
                </TimelineOppositeContent>
                <TimelineSeparator>
                  <TimelineDot color={getActivityColor(activity.type)}>
                    {getActivityIcon(activity.type)}
                  </TimelineDot>
                  {index < displayActivities.length - 1 && <TimelineConnector />}
                </TimelineSeparator>
                <TimelineContent sx={{ py: '12px', px: 2 }}>
                  <Typography variant="body2" component="div">
                    {activity.description}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {activity.user} • {formatTime(activity.timestamp)}
                  </Typography>
                </TimelineContent>
              </TimelineItem>
            ))}
          </Timeline>
        )}
      </CardContent>
    </Card>
  );
};

export default ActivityTimeline;
