import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useCreateBook, useGetAllCategoriesTree } from "../api/hooks";
import { useQueryClient } from "@tanstack/react-query";
import type {
  CreateBookRequestDto,
  CategoryDto,
  PersonDto,
} from "../api/generated/dtos";
import { CategorySelectDropDown } from "../components/CategorySelectDropDown";
import { AuthorsInput } from "../components/AuthorsInput";
import { getGetBooksQueryKey } from "../api/generated/bookStorageAPI";

function CreateBook() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const createBookMutation = useCreateBook();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<CategoryDto | null>(
    null,
  );
  const [authors, setAuthors] = useState<PersonDto[]>([]);

  const { data: categoriesData, isLoading: isLoadingCategories } =
    useGetAllCategoriesTree();

  const categories =
    (categoriesData as { data?: CategoryDto[] } | undefined)?.data ?? [];

  const handleSubmit = async (e: React.SubmitEvent) => {
    e.preventDefault();
    setError(null);

    if (!title.trim()) {
      setError("Title is required");
      return;
    }

    if (!file) {
      setError("Please select a file");
      return;
    }

    setIsSubmitting(true);

    const cat: CategoryDto = {
      id: selectedCategory.id,
      name: selectedCategory.name,
      parentCategoryId: selectedCategory.parentCategoryId,
    };

    try {
      const dto: CreateBookRequestDto = {
        title,
        file,
        description: description || undefined,
        category: cat || undefined,
        authors: authors.length > 0 ? authors : undefined,
      };

      const response = await createBookMutation.mutateAsync({ data: dto });
      const bookId = (response.data as { id?: string } | undefined)?.id;

      queryClient.invalidateQueries({ queryKey: getGetBooksQueryKey() });
      queryClient.invalidateQueries({ queryKey: ["/api/Categories/Tree"] });

      if (bookId) {
        navigate(`/books/${bookId}`);
      } else {
        navigate("/");
      }
    } catch (err) {
      setError("Failed to create book");
      console.error(err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="max-w-xl">
      <Link to="/" className="text-blue-600 hover:underline mb-4 inline-block">
        ← Back to list
      </Link>

      <div className="bg-white rounded-lg shadow p-6">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Add New Book</h1>

        {error && (
          <div className="bg-red-50 text-red-600 p-3 rounded mb-4 text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Title *
            </label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Book File *
            </label>
            <input
              type="file"
              onChange={(e) => setFile(e.target.files?.[0] || null)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Category
            </label>
            {isLoadingCategories ? (
              <div className="text-sm text-gray-500">Loading categories...</div>
            ) : (
              <CategorySelectDropDown
                categories={categories}
                selectedCategory={selectedCategory}
                onChange={setSelectedCategory}
              />
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Authors
            </label>
            <AuthorsInput value={authors} onChange={setAuthors} />
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isSubmitting ? "Creating..." : "Create Book"}
          </button>
        </form>
      </div>
    </div>
  );
}

export default CreateBook;
