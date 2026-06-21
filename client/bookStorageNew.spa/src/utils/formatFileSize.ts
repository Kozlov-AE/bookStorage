/**
 * Formats file size into human-readable format.
 * @param bytes - size of bytes
 * @returns string like: '1.18 МБ', '500 Б', '1.5 ГБ'
 */
export function formatFileSize(bytes: number): string {
    if (bytes < 0) {
        throw new RangeError('Bytes must be greater than 0');
    }
    if (bytes === 0) {
        return '0 Bytes';
    }

    const k = 1024;
    const decimals = 2;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return (bytes / Math.pow(k, i)).toFixed(decimals) + ' ' + sizes[i];
}