export {
  useGetBooks,
  useGetBook,
  useCreateBook,
  useGetAllCategoriesTree,
  useGetAllCategories,
  useCreateCategory,
  useUpdateCategory,
  useDeleteCategory,
  useGetApiPersons,
  useGetApiPersonsSearch,
} from './generated/bookStorageAPI';

export type {
  GetBooksQueryResult,
  GetBookQueryResult,
  CreateBookMutationResult,
  GetAllCategoriesTreeQueryResult,
  getAllCategoriesResponse,
  CreateCategoryMutationResult,
  DeleteCategoryMutationResult,
  GetApiPersonsQueryResult,
  GetApiPersonsSearchQueryResult,
} from './generated/bookStorageAPI';

export type { GetApiPersonsSearchParams } from './generated/dtos';
