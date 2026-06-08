import type { FormEvent } from 'react';
import { XMarkIcon } from './Icons';

interface CreateCategoryModalProps {
  parentName: string | null;
  name: string;
  onNameChange: (name: string) => void;
  onSubmit: (name: string) => void;
  onClose: () => void;
}

export function CreateCategoryModal({ parentName, name, onNameChange, onSubmit, onClose }: CreateCategoryModalProps) {

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const trimmed = name.trim();
    if (!trimmed) return;
    onSubmit(trimmed);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="relative bg-white rounded-xl shadow-2xl w-full max-w-sm mx-4 p-6">
        <button
          onClick={onClose}
          className="absolute top-3 right-3 text-gray-400 hover:text-gray-600"
        >
          <XMarkIcon className="w-5 h-5" />
        </button>

        <h2 className="text-lg font-semibold text-gray-900 mb-1">
          {parentName ? 'New Subcategory' : 'New Category'}
        </h2>
        {parentName && (
          <p className="text-sm text-gray-500 mb-4">
            in <span className="font-medium text-gray-700">{parentName}</span>
          </p>
        )}

        <form onSubmit={handleSubmit}>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Category Name
          </label>
          <input
            type="text"
            value={name}
            onChange={(e) => onNameChange(e.target.value)}
            placeholder="Enter category name"
            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm
                       focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                       placeholder:text-gray-400"
            autoFocus
          />

          <div className="flex justify-end gap-2 mt-5">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100
                         rounded-lg hover:bg-gray-200 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={!name.trim()}
              className="px-4 py-2 text-sm font-medium text-white bg-blue-600
                         rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed
                         transition-colors"
            >
              Create
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
