import {BookOpen, Menu, PlusCircle, Upload, Users, Sun, Moon} from "lucide-react";
import {Button} from "../ui/button/button.tsx";
import {useAppDispatch, useAppSelector} from "../../store/hooks.ts";
import {DARK_THEME_NAME, setSidebarOpen, toggleTheme} from "../../store/uiSlice.ts";
import './header.scss';

function HeaderUi() {
    const theme = useAppSelector(s => s.ui.theme);
    const dispatch = useAppDispatch();

    return (
        <header className='header'>
            <div className='header__left'>
                <Button variant='ghost' size='icon' onClick={() => dispatch(setSidebarOpen(true))}>
                    <Menu/>
                </Button>
                <BookOpen className='header__logo'/>
                <span>BookStorage</span>
                <div className='header__separator'/>
            </div>
            <div className='header__actions'>

            </div>
            <div className='header__spacer'/>
            <Button variant='ghost' size='icon' onClick={() => dispatch(toggleTheme())}>
                {theme === DARK_THEME_NAME ? <Sun/> : <Moon/>}
            </Button>
        </header>
    )
}

export default HeaderUi;