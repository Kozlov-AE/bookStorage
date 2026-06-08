import { Link } from 'react-router-dom';
import { PlusIcon, BookOpenIcon } from './Icons';

export function Header() {
  return (
    <header className="sticky top-0 z-10 bg-white border-b border-gray-200 px-6 py-4">
      <div className="flex items-center justify-between">
        <Link
          to="/books/new"
          className="inline-flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
        >
          <PlusIcon className="w-5 h-5" />
          Add Book
        </Link>

        <Link to="/" className="flex items-center gap-2 text-gray-900 hover:text-blue-600 transition-colors">
          <BookOpenIcon className="w-8 h-8" />
          <span className="text-xl font-bold">BookStorage</span>
        </Link>

        <div className="w-32" />
      </div>
    </header>
  );
}