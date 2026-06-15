import {useAppDispatch, useAppSelector} from "../../store/hooks.ts";
import {setSidebarOpen, toggleTheme} from "../../store/uiSlice.ts";

function HeaderUi() {
    const theme = useAppSelector(s => s.ui.theme);
    const dispatch = useAppDispatch();

    return (
        <header className="header">
            <div className="header__left">
                <button className="header__menu-btn" onClick={() => dispatch(setSidebarOpen(true))} aria-label="Открыть меню">
                    ☰
                </button>
                <h1 className="header__logo">BookStorage</h1>
            </div>
            <div className="header__right">
                <button
                    className="header__theme-btn"
                    onClick={() => dispatch(toggleTheme())}
                    aria-label="Переключить тему"
                >
                    {theme === 'light' ? '🌙' : '☀️'}
                </button>
            </div>
        </header>
    )
}

export default HeaderUi;