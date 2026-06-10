import { Outlet } from 'react-router-dom';
import { Header } from './Header';
import { CategoryTree } from './CategoryTree';
import { useState, useRef, useEffect } from 'react';

const SIDEBAR_MIN = 180;
const SIDEBAR_MAX = 500;
const STORAGE_KEY = 'sidebarWidth';

function getStoredWidth(): number {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const w = Number(stored);
      if (!isNaN(w) && w >= SIDEBAR_MIN && w <= SIDEBAR_MAX) return w;
    }
  } catch { /* ignore */ }
  return 256;
}

export function Layout() {
  const [selectedCategoryId, setSelectedCategoryId] = useState<string | null>(null);
  const [sidebarWidth, setSidebarWidth] = useState(getStoredWidth);
  const draggingRef = useRef(false);

  function handleMouseDown(e: React.MouseEvent) {
    e.preventDefault();
    draggingRef.current = true;
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';

    const handleMouseMove = (e: MouseEvent) => {
      if (!draggingRef.current) return;
      const newWidth = Math.min(SIDEBAR_MAX, Math.max(SIDEBAR_MIN, e.clientX));
      setSidebarWidth(newWidth);
    };

    const handleMouseUp = () => {
      if (!draggingRef.current) return;
      draggingRef.current = false;
      document.body.style.cursor = '';
      document.body.style.userSelect = '';
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };

    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);
  }

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, String(sidebarWidth));
    } catch { /* ignore */ }
  }, [sidebarWidth]);

  return (
    <div className="h-screen bg-white flex flex-col">
      <Header />

      <div className="flex flex-1 min-h-0">
        <div style={{ width: sidebarWidth }} className="flex shrink-0">
          <CategoryTree
            selectedId={selectedCategoryId}
            onSelect={setSelectedCategoryId}
          />

          <div
            onMouseDown={handleMouseDown}
            className="w-1.5 shrink-0 cursor-col-resize hover:bg-blue-400 active:bg-blue-500
                       bg-transparent transition-colors relative -mr-px"
            title="Drag to resize"
          />
        </div>

        <main className="flex-1 p-6 min-w-0 overflow-y-auto">
          <Outlet context={{ selectedCategoryId, setSelectedCategoryId }} />
        </main>
      </div>
    </div>
  );
}