export {
  useGetBooks,
  useGetBook,
  useCreateBook,
  useGetAllCategoriesTree,
  useGetAllCategories,
  useCreateCategory,
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
  GetApiPersonsQueryResult,
  GetApiPersonsSearchQueryResult,
} from './generated/bookStorageAPI';

export type { GetApiPersonsSearchParams } from './generated/dtos';
