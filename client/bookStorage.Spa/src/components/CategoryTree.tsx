import { useState } from 'react';
import {
  ChevronRightIcon, ChevronDownIcon,
  FolderIcon, FolderOpenIcon,
  LoaderIcon, PlusIcon, TrashIcon, PencilIcon,
} from '../components/Icons';
import { useGetAllCategoriesTree, useCreateCategory, useUpdateCategory, useDeleteCategory } from '../api/hooks';
import { useQueryClient } from '@tanstack/react-query';
import type { CategoryDto } from '../api/generated/dtos';
import { CreateCategoryModal } from './CreateCategoryModal';
import { EditCategoryModal } from './EditCategoryModal';

interface CategoryNodeProps {
  category: CategoryDto;
  level: number;
  selectedId: string | null;
  expandedIds: Set<string>;
  onSelect: (id: string | null) => void;
  onToggle: (id: string) => void;
  onAddChild: (parentId: string, parentName: string) => void;
  onEdit: (id: string, name: string) => void;
  onDelete: (id: string, name: string) => void;
}

function CategoryNode({ category, level, selectedId, expandedIds, onSelect, onToggle, onAddChild, onEdit, onDelete }: CategoryNodeProps) {
  const hasChildren = category.subCategories && category.subCategories.length > 0;
  const isExpanded = expandedIds.has(category.id ?? '');
  const isSelected = selectedId === category.id;

  return (
    <div className="group">
      <div
        className={`flex items-center rounded-md transition-colors ${
          isSelected ? 'bg-blue-100' : 'hover:bg-gray-100'
        }`}
        style={{ paddingLeft: `${level * 16 + 8}px` }}
      >
        <button
          onClick={() => {
            onSelect(category.id || null);
            if (hasChildren && category.id) {
              onToggle(category.id);
            }
          }}
          className={`flex items-center gap-1 flex-1 py-1.5 text-left text-sm ${
            isSelected ? 'text-blue-700 font-medium' : 'text-gray-700'
          }`}
        >
          {hasChildren ? (
            <span className="w-4 h-4 flex items-center justify-center shrink-0">
              {isExpanded ? (
                <ChevronDownIcon className="w-4 h-4" />
              ) : (
                <ChevronRightIcon className="w-4 h-4" />
              )}
            </span>
          ) : (
            <span className="w-4 shrink-0" />
          )}

          {isExpanded ? (
            <FolderOpenIcon className="w-4 h-4 text-yellow-500 shrink-0" />
          ) : (
            <FolderIcon className="w-4 h-4 text-yellow-500 shrink-0" />
          )}

          <span className="truncate">{category.name}</span>
        </button>

        {category.id && (
          <>
            <button
              onClick={() => onEdit(category.id!, category.name)}
              className="p-1 rounded-md text-gray-400 opacity-0 group-hover:opacity-100
                         hover:text-green-600 hover:bg-green-50 transition-all shrink-0"
              title="Edit category"
            >
              <PencilIcon className="w-3.5 h-3.5" />
            </button>
            <button
              onClick={() => onDelete(category.id!, category.name)}
              className="p-1 rounded-md text-gray-400 opacity-0 group-hover:opacity-100
                         hover:text-red-600 hover:bg-red-50 transition-all shrink-0"
              title="Delete category"
            >
              <TrashIcon className="w-3.5 h-3.5" />
            </button>
            <button
              onClick={() => onAddChild(category.id, category.name)}
              className="p-1 mr-1 rounded-md text-gray-400 opacity-0 group-hover:opacity-100
                         hover:text-blue-600 hover:bg-blue-50 transition-all shrink-0"
              title="Add subcategory"
            >
              <PlusIcon className="w-3.5 h-3.5" />
            </button>
          </>
        )}
      </div>

      {hasChildren && isExpanded && (
        <div>
          {category.subCategories!.map((child) => (
            <CategoryNode
              key={child.id}
              category={child}
              level={level + 1}
              selectedId={selectedId}
              expandedIds={expandedIds}
              onSelect={onSelect}
              onToggle={onToggle}
              onAddChild={onAddChild}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </div>
      )}
    </div>
  );
}

function findCategoryInTree(tree: CategoryDto[] | undefined | null, id: string): CategoryDto | null {
  if (!tree) return null;
  for (const cat of tree) {
    if (cat.id === id) return cat;
    const found = findCategoryInTree(cat.subCategories, id);
    if (found) return found;
  }
  return null;
}

function findParentOf(tree: CategoryDto[] | undefined | null, childId: string): CategoryDto | null {
  if (!tree) return null;
  for (const cat of tree) {
    if (cat.subCategories?.some(c => c.id === childId)) return cat;
    const found = findParentOf(cat.subCategories, childId);
    if (found) return found;
  }
  return null;
}

function hasDuplicateName(tree: CategoryDto[] | undefined | null, categoryId: string, newName: string): boolean {
  if (!tree) return false;
  const parent = findParentOf(tree, categoryId);
  const siblings = parent ? (parent.subCategories ?? []) : tree;
  return siblings.some(c => c.id !== categoryId && c.name === newName);
}

function hasSiblingName(tree: CategoryDto[] | undefined | null, parentId: string | null, name: string): boolean {
  if (!tree) return false;
  const siblings = parentId
    ? (findCategoryInTree(tree, parentId)?.subCategories ?? [])
    : tree;
  return siblings.some(c => c.name === name);
}

interface CategoryToEdit {
  id: string;
  name: string;
  parentCategoryId: string | null;
}

interface CategoryTreeProps {
  selectedId: string | null;
  onSelect: (id: string | null) => void;
}

export function CategoryTree({ selectedId, onSelect }: CategoryTreeProps) {
  const queryClient = useQueryClient();
  const { data, isLoading, error, refetch } = useGetAllCategoriesTree({
    query: {
      retry: 3,
      retryDelay: attemptIndex => Math.min(1000 * 2 ** attemptIndex, 30000),
    },
  });

  const categories = (data as unknown as { data?: CategoryDto[] } | undefined)?.data;

  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());

  function handleToggle(id: string) {
    setExpandedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  const [modalState, setModalState] = useState<{
    parentId: string | null;
    parentName: string | null;
  } | null>(null);

  const [modalName, setModalName] = useState('');
  const [createError, setCreateError] = useState<string | null>(null);

  const createCategoryMutation = useCreateCategory({
    mutation: {
      onSuccess: (result) => {
        if (result.status === 200 && result.data?.id) {
          onSelect(result.data.id);
          if (result.data.parentCategoryId) {
            setExpandedIds(prev => {
              const next = new Set(prev);
              next.add(result.data.parentCategoryId!);
              return next;
            });
          }
        }
        setModalName('');
        setCreateError(null);
        queryClient.invalidateQueries({ queryKey: ['/api/Categories/Tree'] });
      },
    },
  });

  const deleteCategoryMutation = useDeleteCategory({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/api/Categories/Tree'] });
        onSelect(null);
      },
    },
  });

  const [editCategory, setEditCategory] = useState<CategoryToEdit | null>(null);
  const [editError, setEditError] = useState<string | null>(null);

  const updateCategoryMutation = useUpdateCategory({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/api/Categories/Tree'] });
        setEditCategory(null);
        setEditError(null);
      },
    },
  });

  function handleEdit(id: string, name: string) {
    const parent = findParentOf(categories, id);
    setEditCategory({ id, name, parentCategoryId: parent?.id ?? null });
    setEditError(null);
  }

  function handleEditSubmit(newName: string) {
    if (!editCategory) return;

    const trimmed = newName.trim();
    if (!trimmed) {
      setEditError('Название категории не может быть пустым');
      return;
    }

    if (trimmed !== editCategory.name && hasDuplicateName(categories, editCategory.id, trimmed)) {
      setEditError('Категория с таким именем уже существует на этом уровне');
      return;
    }

    setEditError(null);
    updateCategoryMutation.mutate({
      id: editCategory.id,
      data: { name: trimmed, parentCategoryId: editCategory.parentCategoryId },
    });
  }

  function handleDelete(id: string, name: string) {
    const cat = findCategoryInTree(categories, id);
    if (cat?.subCategories?.length) {
      window.alert("Нельзя удалить категорию, имеющую дочерние категории!");
      return;
    }
    if (window.confirm(`Удаление категории "${name}"?`)) {
      deleteCategoryMutation.mutate({ id });
    }
  }

  function handleAddChild(parentId: string, parentName: string) {
    setModalState({ parentId, parentName });
  }

  function handleAddRoot() {
    setModalState({ parentId: null, parentName: null });
  }

  function handleModalSubmit(name: string) {
    if (!modalState) return;

    const trimmed = name.trim();
    if (!trimmed) {
      setCreateError('Название категории не может быть пустым');
      return;
    }

    if (hasSiblingName(categories, modalState.parentId, trimmed)) {
      setCreateError('Категория с таким именем уже существует на этом уровне');
      return;
    }

    setCreateError(null);
    createCategoryMutation.mutate({
      data: {
        name: trimmed,
        parentCategoryId: modalState.parentId,
      },
    });
    setModalState(null);
  }

  function handleModalClose() {
    setModalState(null);
    setCreateError(null);
  }

  function handleRetry() {
    refetch();
  }

  return (
    <div className="w-full bg-gray-50 border-r border-gray-200 p-3 overflow-y-auto flex flex-col">
      <div className="flex items-center justify-between mb-3 px-2 shrink-0">
        <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wide">
          Categories
        </h2>
        <button
          onClick={handleRetry}
          disabled={isLoading}
          className="text-blue-600 hover:text-blue-800 disabled:text-blue-400 disabled:opacity-50 text-xs font-medium"
        >
          {isLoading ? (
            <span className="inline-flex items-center gap-1">
              <LoaderIcon className="w-3 h-3 animate-spin" />
              Refreshing...
            </span>
          ) : null}
        </button>
      </div>

      <div className="space-y-0.5 flex-1">
        {isLoading ? (
          <div className="flex flex-col items-center justify-center py-8">
            <LoaderIcon className="w-6 h-6 text-gray-400 animate-spin" />
            <p className="text-gray-500 text-sm mt-2">Loading categories...</p>
          </div>
        ) : error ? (
          <div className="text-center py-8">
            <div className="w-12 h-12 text-red-400 mx-auto mb-3">
              <FolderIcon className="w-full h-full" />
            </div>
            <p className="text-red-500 text-sm mb-3">Failed to load categories</p>
            <button
              onClick={handleRetry}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 text-sm"
            >
              Retry
            </button>
          </div>
        ) : (
          <div>
            <button
              onClick={() => onSelect(null)}
              className={`w-full flex items-center gap-2 px-2 py-1.5 rounded-md text-sm transition-colors ${
                selectedId === null
                  ? 'bg-blue-100 text-blue-700 font-medium'
                  : 'hover:bg-gray-100 text-gray-700'
              }`}
            >
              <FolderIcon className="w-4 h-4 text-gray-400" />
              <span>All Books</span>
            </button>

            {categories && categories.length > 0 ? (
              categories.map((category) => (
                <CategoryNode
                  key={category.id}
                  category={category}
                  level={0}
                  selectedId={selectedId}
                  expandedIds={expandedIds}
                  onSelect={onSelect}
                  onToggle={handleToggle}
                  onAddChild={handleAddChild}
                  onEdit={handleEdit}
                  onDelete={handleDelete}
                />
              ))
            ) : (
              <div className="text-center py-8">
                <FolderIcon className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <p className="text-gray-400 text-sm">No categories found</p>
              </div>
            )}
          </div>
        )}
      </div>

      <button
        onClick={handleAddRoot}
        className="mt-3 flex items-center justify-center gap-1.5 w-full px-3 py-2
                   text-sm font-medium text-blue-600 bg-blue-50 rounded-lg
                   hover:bg-blue-100 transition-colors shrink-0"
      >
        <PlusIcon className="w-4 h-4" />
        <span>Add Category</span>
      </button>

      {modalState && (
        <CreateCategoryModal
          parentName={modalState.parentName}
          name={modalName}
          error={createError}
          onNameChange={(v) => { setModalName(v); setCreateError(null); }}
          onSubmit={handleModalSubmit}
          onClose={handleModalClose}
        />
      )}

      {editCategory && (
        <EditCategoryModal
          categoryName={editCategory.name}
          error={editError}
          onSubmit={handleEditSubmit}
          onClose={() => { setEditCategory(null); setEditError(null); }}
          onClearError={() => setEditError(null)}
        />
      )}
    </div>
  );
}