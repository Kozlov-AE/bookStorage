
import { FolderIcon } from '../components/Icons';

export function Categories() {
  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Categories</h1>

      <div className="bg-gray-50 rounded-lg p-8 text-center">
        <FolderIcon className="w-12 h-12 mx-auto mb-3 text-gray-300" />
        <p className="text-gray-500">Category management coming soon</p>
        <p className="text-sm text-gray-400 mt-2">Backend endpoint not yet implemented</p>
      </div>
    </div>
  );
}