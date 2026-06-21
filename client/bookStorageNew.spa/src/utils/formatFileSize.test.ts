import {formatFileSize} from "./formatFileSize";

describe('formatFileSize', () => {
    describe('base values', () => {
        it('should returns 0 Bytes', () => {
            expect(formatFileSize(0)).toBe('0 Bytes');
        })
        it('should return 1.00 Bytes', () => {
            expect(formatFileSize(1)).toBe('1.00 Bytes');
        })
        it('should return 1.00 MB', () => {
            const val = 1024*1024;
            expect(formatFileSize(val)).toBe('1.00 MB');
        })
        it('should return 1,50 MB', () => {
            const val = 1024*1024*1.5;
            expect(formatFileSize(val)).toBe('1.50 MB');
        })
        it('should return 5.25 GB', () => {
            const val = 1024*1024*1024*5.25;
            expect(formatFileSize(val)).toBe('5.25 GB');
        })
        it('should return exception', () => {
            const val = -1;
            expect(() => formatFileSize(val)).toThrow(`Bytes must be greater than 0`)
        })
    })
})