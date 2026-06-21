import {toggleTheme, setSidebarOpen, DARK_THEME_NAME, LIGHT_THEME_NAME, LOCALSTORAGE_THEME_KEY} from "./uiSlice";
import uiSlice from "./uiSlice";

describe('uiSlice', () => {
    describe('check uiState default theme from localStorage', () => {
        it('should read theme from localStorage', async () => {
            vi.resetModules();
            localStorage.setItem(LOCALSTORAGE_THEME_KEY, LIGHT_THEME_NAME);

            const uiSlice = (await import('./uiSlice')).default;

            expect(uiSlice.getInitialState().theme).toBe(LIGHT_THEME_NAME);
        })
    })
    describe('toggle theme', () => {
        it('should set dark theme', () => {
            const newState = uiSlice.reducer({theme: LIGHT_THEME_NAME, sidebarOpen: true}, toggleTheme());
            const theme = localStorage.getItem(LOCALSTORAGE_THEME_KEY);

            expect(newState.theme).toBe(DARK_THEME_NAME);
            expect(theme).toBeTruthy();
            expect(theme).toBe(DARK_THEME_NAME);
        })
        it('should set light theme', () => {
            const newState = uiSlice.reducer({theme: DARK_THEME_NAME, sidebarOpen: true}, toggleTheme());
            const theme = localStorage.getItem(LOCALSTORAGE_THEME_KEY);

            expect(newState.theme).toBe(LIGHT_THEME_NAME);
            expect(theme).toBeTruthy();
            expect(theme).toBe(LIGHT_THEME_NAME);
        })
    })
    describe('setSidebarOpen', () => {
        it('should set sidebar open', () => {
            const newState = uiSlice.reducer({theme: LIGHT_THEME_NAME, sidebarOpen: false}, setSidebarOpen(true));

            expect(newState.sidebarOpen).toBe(true);
        })

    })
})