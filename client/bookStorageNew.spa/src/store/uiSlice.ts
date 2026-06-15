import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

export const LOCALSTORAGE_THEME_KEY = 'theme';
export const DARK_THEME_NAME = 'dark';
export const LIGHT_THEME_NAME = 'light';

type Theme = 'light' | 'dark';

interface UiState {
    theme: Theme;
    sidebarOpen: boolean;
}

function getInitialTheme(): Theme {
    const stored = localStorage.getItem(LOCALSTORAGE_THEME_KEY) as Theme | null;
    if (stored) return stored;
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? DARK_THEME_NAME : LIGHT_THEME_NAME;
}

const initialState: UiState = {
    theme: getInitialTheme(),
    sidebarOpen: false,
}

const uiSlice = createSlice({
    name: 'ui',
    initialState,
    reducers: {
        toggleTheme: (state: UiState) => {
            state.theme = state.theme === DARK_THEME_NAME ? LIGHT_THEME_NAME : DARK_THEME_NAME;
            localStorage.setItem(LOCALSTORAGE_THEME_KEY, state.theme);
        },
        setSidebarOpen(state: UiState, action: PayloadAction<boolean>) {
            state.sidebarOpen = action.payload;
        },
    },
})

export const {toggleTheme, setSidebarOpen} = uiSlice.actions;
export default uiSlice;