import { useParams, Link } from 'react-router-dom';
import { useGetBook } from '../api/hooks';
import type { getBookResponse } from '../api/generated/bookStorageAPI';

function formatDate(dateStr: string | null | undefined): string {
  if (!dateStr) return "";
  return new Date(dateStr).toLocaleDateString("ru-RU", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function BookDetail() {
  const { id } = useParams<{ id: string }>();
  const { data, isLoading, error } = useGetBook(id!) as { data: getBookResponse | undefined; isLoading: boolean; error: unknown };

  const book = data?.data;

  if (isLoading) {
    return <div className="text-gray-500">Loading...</div>;
  }

  if (error || !book) {
    return <div className="text-red-500">Book not found</div>;
  }

  return (
    <div className="max-w-2xl">
      <Link to="/" className="text-blue-600 hover:underline mb-4 inline-block">
        ← Back to list
      </Link>

      <div className="bg-white rounded-lg shadow-md p-6">
        <h1 className="text-2xl font-bold text-gray-900 mb-4">{book.title}</h1>

        <div className="flex gap-4 text-xs text-gray-400 mb-4">
          <span>Created: {formatDate(book.createdAt)}</span>
          {book.updatedAt && (
            <span>Updated: {formatDate(book.updatedAt)}</span>
          )}
        </div>

        {book.description && (
          <p className="text-gray-600 mb-4">{book.description}</p>
        )}

        {book.category && (
          <div className="mb-4">
            <span className="inline-block bg-blue-100 text-blue-800 text-xs font-medium px-2.5 py-1 rounded-full">
              {book.category.name}
            </span>
          </div>
        )}

        <div className="border-t pt-4">
          <h2 className="text-lg font-semibold mb-2">Files</h2>
          {book.files && book.files.length > 0 ? (
            <ul className="space-y-2">
              {book.files.map((file) => (
                <li key={file.id} className="flex items-center gap-2 text-sm">
                  {file.downloadUrl ? (
                    <a href={file.downloadUrl} className="text-blue-600 hover:underline">
                      {file.fileName}{file.fileType ? `.${file.fileType}` : ""}
                    </a>
                  ) : (
                    <span>{file.fileName}</span>
                  )}
                  <span className="text-gray-400">
                    ({Math.round(Number(file.fileSizeBytes) / 1024)} KB)
                  </span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-gray-400 text-sm">No files</p>
          )}
        </div>

        {book.authors && book.authors.length > 0 && (
          <div className="border-t pt-4 mt-4">
            <h2 className="text-lg font-semibold mb-2">Authors</h2>
            <ul className="space-y-1">
              {book.authors.map((author, idx) => (
                <li key={idx} className="text-gray-700">
                  {author.fullName}
                  {author.birthday && (
                    <span className="text-gray-400 text-sm ml-2">
                      ({author.birthday})
                    </span>
                  )}
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
}