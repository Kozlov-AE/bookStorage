import { CategoryDto } from "../api/generated/dtos";

interface CategorySelectDropDownProps {
  categories: CategoryDto[];
  selectedCategory: CategoryDto | null;
  onChange: (category: CategoryDto | null) => void;
}

interface FlatCategory {
  category: CategoryDto;
  level: number;
}

function flattenCategories(categories: CategoryDto[], level = 0): FlatCategory[] {
  const result: FlatCategory[] = [];
  for (const cat of categories) {
    result.push({ category: cat, level });
    if (cat.subCategories && cat.subCategories.length > 0) {
      result.push(...flattenCategories(cat.subCategories, level + 1));
    }
  }
  return result;
}

export function CategorySelectDropDown({
  categories,
  selectedCategory,
  onChange,
}: CategorySelectDropDownProps) {
  const flatCategories = flattenCategories(categories);

  return (
    <div className="border border-gray-200 rounded-lg max-h-60 overflow-y-auto">
      {flatCategories.length > 0 ? (
        flatCategories.map((item) => (
          <button
            key={item.category.id}
            type="button"
            onClick={() => onChange(item.category)}
            className={`w-full text-left px-3 py-2 text-sm flex items-center gap-2 ${
              selectedCategory?.id === item.category.id
                ? "bg-blue-100 text-blue-700 font-medium"
                : "text-gray-700 hover:bg-gray-50"
            }`}
            style={{ paddingLeft: `${item.level * 20 + 12}px` }}
          >
            {item.category.name}
          </button>
        ))
      ) : (
        <div className="p-4 text-center text-gray-400 text-sm">
          No categories available
        </div>
      )}
    </div>
  );
}
