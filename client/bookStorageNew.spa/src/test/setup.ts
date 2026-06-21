import '@testing-library/jest-dom';
import { vi } from 'vitest';

vi.stubGlobal('matchMedia', (query: string) => ({
    matches: true,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeAllListener: () => {},
    dispatchEvent: () => {},
}));