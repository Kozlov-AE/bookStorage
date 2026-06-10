import { useState, type FormEvent } from 'react';
import { XMarkIcon } from './Icons';

interface EditCategoryModalProps {
  categoryName: string;
  error?: string | null;
  onSubmit: (name: string) => void;
  onClose: () => void;
  onClearError?: () => void;
}

export function EditCategoryModal({ categoryName, error, onSubmit, onClose, onClearError }: EditCategoryModalProps) {
  const [name, setName] = useState(categoryName);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const trimmed = name.trim();
    if (!trimmed) return;
    onSubmit(trimmed);
  };

  const handleNameChange = (value: string) => {
    setName(value);
    if (error && onClearError) {
      onClearError();
    }
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

        <h2 className="text-lg font-semibold text-gray-900 mb-4">
          Новое название категории «{categoryName}»
        </h2>

        <form onSubmit={handleSubmit}>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Название категории
          </label>
          <input
            type="text"
            value={name}
            onChange={(e) => handleNameChange(e.target.value)}
            placeholder="Введите новое название"
            className={`w-full px-3 py-2 border rounded-lg text-sm
                       focus:outline-none focus:ring-2 focus:border-blue-500
                       placeholder:text-gray-400 ${
              error
                ? 'border-red-400 focus:ring-red-500'
                : 'border-gray-300 focus:ring-blue-500'
            }`}
            autoFocus
          />

          {error && (
            <p className="mt-1.5 text-sm text-red-600">{error}</p>
          )}

          <div className="flex justify-end gap-2 mt-5">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100
                         rounded-lg hover:bg-gray-200 transition-colors"
            >
              Отмена
            </button>
            <button
              type="submit"
              disabled={!name.trim()}
              className="px-4 py-2 text-sm font-medium text-white bg-blue-600
                         rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed
                         transition-colors"
            >
              Сохранить
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
