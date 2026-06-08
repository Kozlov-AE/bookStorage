import * as reactRouterDom from "react-router-dom";
import { Link } from "react-router-dom";
import { useGetBooks } from "../api/hooks";
import { BookIcon } from "../components/Icons";
import type { getBooksResponse } from "../api/generated/bookStorageAPI";

interface OutletContext {
  selectedCategoryId: string | null;
  setSelectedCategoryId: (id: string | null) => void;
}

export function BookList() {
  const { selectedCategoryId } =
    reactRouterDom.useOutletContext<OutletContext>();
  const { data, isLoading, error } = useGetBooks() as {
    data: getBooksResponse | undefined;
    isLoading: boolean;
    error: unknown;
  };

  const books = data?.data ?? [];

  if (isLoading) {
    return <div className="text-gray-500">Loading books...</div>;
  }

  if (error) {
    return <div className="text-red-500">Failed to load books</div>;
  }

  const filteredBooks = selectedCategoryId
    ? books.filter((book) => book.categoryId === selectedCategoryId)
    : books;

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Books</h1>

      {filteredBooks.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredBooks.map((book) => (
            <Link
              key={book.id}
              to={`/books/${book.id}`}
              className="block bg-white rounded-lg shadow hover:shadow-md transition-shadow p-4"
            >
              <div className="flex items-start gap-3">
                <BookIcon className="w-8 h-8 text-blue-500 shrink-0" />
                <div className="min-w-0">
                  <h3 className="font-semibold text-gray-900 truncate">
                    {book.title}
                  </h3>
                </div>
              </div>
            </Link>
          ))}
        </div>
      ) : (
        <div className="text-gray-400 text-center py-12">
          <BookIcon className="w-12 h-12 mx-auto mb-3 opacity-50" />
          <p>No books found</p>
        </div>
      )}
    </div>
  );
}
