import JSZip from 'jszip';

/**
 * Downloads a single file with the specified filename and content.
 * Creates a temporary download link and triggers the download.
 * 
 * @param filename - The name of the file to download (e.g., 'Entity.cs')
 * @param content - The content of the file
 */
export const downloadFile = (filename: string, content: string): void => {
  const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
};

/**
 * Downloads multiple files as a ZIP archive.
 * Each file is added to the ZIP with its original name.
 * 
 * @param files - Array of files with name and code properties
 * @param zipName - Name of the ZIP file (default: 'generated-code.zip')
 * @returns Promise that resolves when download is complete
 */
export const downloadAllAsZip = async (
  files: Array<{ name: string; code: string }>,
  zipName: string = 'generated-code.zip'
): Promise<void> => {
  const zip = new JSZip();

  // Add each file to the ZIP
  files.forEach((file) => {
    zip.file(file.name, file.code);
  });

  // Generate the ZIP blob
  const blob = await zip.generateAsync({ type: 'blob' });
  
  // Create download link
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = zipName;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
};

/**
 * Gets the appropriate file extension based on language.
 * Used for dynamic file naming when language changes.
 * 
 * @param language - Monaco language identifier
 * @returns File extension including the dot (e.g., '.cs', '.ts')
 */
export const getFileExtension = (language: string): string => {
  const extensions: Record<string, string> = {
    csharp: '.cs',
    typescript: '.ts',
    javascript: '.js',
    sql: '.sql',
    json: '.json',
  };
  
  return extensions[language] || '.txt';
};
