/**
 * ConnectionFormDialog Component
 * Dialog wrapper for the connection form
 */

import { Dialog, DialogTitle, DialogContent } from '@mui/material';
import ConnectionForm from './ConnectionForm';
import type { ConnectionFormData } from './ConnectionForm';

interface ConnectionFormDialogProps {
  open: boolean;
  onClose: () => void;
  initialData?: ConnectionFormData;
  onSubmit: (data: ConnectionFormData) => Promise<void>;
  onTestConnection: (connectionString: string) => Promise<boolean>;
}

export default function ConnectionFormDialog({
  open,
  onClose,
  initialData,
  onSubmit,
  onTestConnection,
}: ConnectionFormDialogProps) {
  const handleSubmit = async (data: ConnectionFormData) => {
    await onSubmit(data);
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {initialData?.id ? 'Edit Connection' : 'Add Connection'}
      </DialogTitle>
      <DialogContent>
        <ConnectionForm
          initialData={initialData}
          onSubmit={handleSubmit}
          onCancel={onClose}
          onTestConnection={onTestConnection}
        />
      </DialogContent>
    </Dialog>
  );
}
