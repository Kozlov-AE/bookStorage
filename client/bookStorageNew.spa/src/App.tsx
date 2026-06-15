import { useAppSelector } from "./store/hooks.ts";
import { DARK_THEME_NAME } from "./store/uiSlice.ts";
import {BrowserRouter, Route, Routes} from "react-router-dom";
import AppLayout from "./components/layout/appLayout.tsx";
import BooksPage from "./pages/Books/BooksPage.tsx";
import AuthorsPage from "./pages/Authors/AuthorsPage.tsx";
import BookDetailsPage from "./pages/BookDetail/BookDetailsPage.tsx";
import {useEffect} from "react";


function App() {
  const theme = useAppSelector(s => s.ui.theme)

  useEffect(() => {
      document.documentElement.classList.toggle(DARK_THEME_NAME, theme === DARK_THEME_NAME);
  }, [theme])

  return (
      <BrowserRouter>
          <Routes>
              <Route element={<AppLayout />}>
                  <Route path="/" element={<BooksPage />} />
                  <Route path="/authors" element={<AuthorsPage />}/>
                  <Route path="/books/:id" element={<BookDetailsPage />} />
              </Route>
          </Routes>
      </BrowserRouter>
  )
}

export default App;
