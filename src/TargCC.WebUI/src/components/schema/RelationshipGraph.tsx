import { Paper, Typography, Box } from '@mui/material';
import type { DatabaseSchema, Table, Relationship } from '../../types/schema';

/**
 * Props for RelationshipGraph component
 */
interface RelationshipGraphProps {
  /** Database schema with tables and relationships */
  schema: DatabaseSchema;
}

/**
 * Position of a table in the graph
 */
interface TablePosition {
  x: number;
  y: number;
}

/**
 * Props for TableBox component
 */
interface TableBoxProps {
  table: Table;
  position: TablePosition;
}

/**
 * Props for RelationshipLine component
 */
interface RelationshipLineProps {
  relationship: Relationship;
  positions: Record<string, TablePosition>;
}

/**
 * Calculate positions for all tables in a grid layout
 */
function calculateTablePositions(tables: Table[]): Record<string, TablePosition> {
  const cols = Math.ceil(Math.sqrt(tables.length));
  const spacing = { x: 200, y: 180 };

  return tables.reduce(
    (acc, table, idx) => {
      acc[table.name] = {
        x: (idx % cols) * spacing.x + 60,
        y: Math.floor(idx / cols) * spacing.y + 60,
      };
      return acc;
    },
    {} as Record<string, TablePosition>
  );
}

/**
 * Table Box Component
 * Renders a single table as an SVG box
 */
function TableBox({ table, position }: TableBoxProps) {
  const width = 160;
  const height = 80;
  const columnCount = table.columns.length;

  return (
    <g>
      {/* Table Rectangle */}
      <rect
        x={position.x}
        y={position.y}
        width={width}
        height={height}
        fill="white"
        stroke="#1976d2"
        strokeWidth="2"
        rx="4"
      />

      {/* Table Name */}
      <text
        x={position.x + width / 2}
        y={position.y + 25}
        textAnchor="middle"
        fontSize="14"
        fontWeight="bold"
        fill="#1976d2"
      >
        {table.name}
      </text>

      {/* Schema Name */}
      <text
        x={position.x + width / 2}
        y={position.y + 45}
        textAnchor="middle"
        fontSize="11"
        fill="#666"
      >
        {table.schema}
      </text>

      {/* Column Count */}
      <text
        x={position.x + width / 2}
        y={position.y + 65}
        textAnchor="middle"
        fontSize="10"
        fill="#999"
      >
        {columnCount} column{columnCount !== 1 ? 's' : ''}
      </text>

      {/* TargCC Badge */}
      {table.hasTargCCColumns && (
        <g>
          <rect
            x={position.x + width - 45}
            y={position.y + 5}
            width={40}
            height={16}
            fill="#4caf50"
            rx="8"
          />
          <text
            x={position.x + width - 25}
            y={position.y + 16}
            textAnchor="middle"
            fontSize="9"
            fill="white"
            fontWeight="bold"
          >
            TargCC
          </text>
        </g>
      )}
    </g>
  );
}

/**
 * Relationship Line Component
 * Renders a relationship as an SVG line with arrow
 */
function RelationshipLine({ relationship, positions }: RelationshipLineProps) {
  const fromPos = positions[relationship.fromTable];
  const toPos = positions[relationship.toTable];

  if (!fromPos || !toPos) {
    return null;
  }

  // Calculate line endpoints (center of boxes)
  const fromX = fromPos.x + 80;
  const fromY = fromPos.y + 40;
  const toX = toPos.x + 80;
  const toY = toPos.y + 40;

  // Determine line color based on relationship type
  const color = relationship.type === 'one-to-many' ? '#ff9800' : '#2196f3';

  return (
    <g>
      {/* Line */}
      <line
        x1={fromX}
        y1={fromY}
        x2={toX}
        y2={toY}
        stroke={color}
        strokeWidth="2"
        markerEnd="url(#arrowhead)"
      />

      {/* Relationship label */}
      <text
        x={(fromX + toX) / 2}
        y={(fromY + toY) / 2 - 5}
        textAnchor="middle"
        fontSize="10"
        fill={color}
        fontWeight="medium"
      >
        {relationship.type}
      </text>
    </g>
  );
}

/**
 * Relationship Graph Component
 * Visualizes database relationships using SVG
 */
export default function RelationshipGraph({ schema }: RelationshipGraphProps) {
  const tablePositions = calculateTablePositions(schema.tables);

  // Calculate SVG dimensions based on table positions
  const maxX = Math.max(...Object.values(tablePositions).map((p) => p.x)) + 200;
  const maxY = Math.max(...Object.values(tablePositions).map((p) => p.y)) + 150;
  const width = Math.max(800, maxX);
  const height = Math.max(600, maxY);

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom sx={{ mb: 2 }}>
        Relationship Diagram
      </Typography>

      <Box sx={{ overflow: 'auto', border: '1px solid #e0e0e0', borderRadius: 1 }}>
        <svg width={width} height={height} style={{ display: 'block' }}>
          {/* Define arrow marker */}
          <defs>
            <marker
              id="arrowhead"
              markerWidth="10"
              markerHeight="10"
              refX="9"
              refY="3"
              orient="auto"
            >
              <polygon points="0 0, 10 3, 0 6" fill="#ff9800" />
            </marker>
          </defs>

          {/* Draw relationships first (lines behind boxes) */}
          {schema.relationships.map((rel, idx) => (
            <RelationshipLine
              key={`rel-${idx}`}
              relationship={rel}
              positions={tablePositions}
            />
          ))}

          {/* Draw tables on top */}
          {schema.tables.map((table) => (
            <TableBox key={table.name} table={table} position={tablePositions[table.name]} />
          ))}
        </svg>
      </Box>

      {schema.relationships.length === 0 && (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2, textAlign: 'center' }}>
          No relationships defined in schema
        </Typography>
      )}
    </Paper>
  );
}
