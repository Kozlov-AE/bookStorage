import { useState, useCallback } from 'react';
import {
  ChevronRightIcon, ChevronDownIcon,
  FolderIcon, FolderOpenIcon,
  LoaderIcon, PlusIcon,
} from '../components/Icons';
import { useGetAllCategoriesTree, useCreateCategory } from '../api/hooks';
import { useQueryClient } from '@tanstack/react-query';
import type { CategoryDto } from '../api/generated/dtos';
import { CreateCategoryModal } from './CreateCategoryModal';

interface CategoryNodeProps {
  category: CategoryDto;
  level: number;
  selectedId: string | null;
  expandedIds: Set<string>;
  onSelect: (id: string | null) => void;
  onToggle: (id: string) => void;
  onAddChild: (parentId: string, parentName: string) => void;
}

function CategoryNode({ category, level, selectedId, expandedIds, onSelect, onToggle, onAddChild }: CategoryNodeProps) {
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
          <button
            onClick={() => onAddChild(category.id, category.name)}
            className="p-1 mr-1 rounded-md text-gray-400 opacity-0 group-hover:opacity-100
                       hover:text-blue-600 hover:bg-blue-50 transition-all shrink-0"
            title="Add subcategory"
          >
            <PlusIcon className="w-3.5 h-3.5" />
          </button>
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
            />
          ))}
        </div>
      )}
    </div>
  );
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

  const handleToggle = useCallback((id: string) => {
    setExpandedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  const [modalState, setModalState] = useState<{
    parentId: string | null;
    parentName: string | null;
  } | null>(null);

  const [modalName, setModalName] = useState('');

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
        queryClient.invalidateQueries({ queryKey: ['/api/Categories/Tree'] });
      },
    },
  });

  const handleAddChild = useCallback((parentId: string, parentName: string) => {
    setModalState({ parentId, parentName });
  }, []);

  const handleAddRoot = useCallback(() => {
    setModalState({ parentId: null, parentName: null });
  }, []);

  const handleModalSubmit = useCallback((name: string) => {
    if (!modalState) return;
    createCategoryMutation.mutate({
      data: {
        name,
        parentCategoryId: modalState.parentId,
      },
    });
    setModalState(null);
  }, [modalState, createCategoryMutation]);

  const handleModalClose = useCallback(() => {
    setModalState(null);
  }, []);

  const handleRetry = useCallback(() => {
    refetch();
  }, [refetch]);

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
          onNameChange={setModalName}
          onSubmit={handleModalSubmit}
          onClose={handleModalClose}
        />
      )}
    </div>
  );
}