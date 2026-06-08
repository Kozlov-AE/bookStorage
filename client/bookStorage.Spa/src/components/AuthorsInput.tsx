import AsyncCreatableSelect from "react-select/async-creatable";
import type { PersonDto } from "../api/generated/dtos";
import { getApiPersonsSearch } from "../api/generated/bookStorageAPI";

interface AuthorOption {
  label: string;
  value: PersonDto;
}

interface AuthorsInputProps {
  value: PersonDto[];
  onChange: (authors: PersonDto[]) => void;
}

const mapToOption = (person: PersonDto): AuthorOption => ({
  label: person.fullName,
  value: person,
});

export function AuthorsInput({ value, onChange }: AuthorsInputProps) {
  const loadOptions = async (input: string): Promise<AuthorOption[]> => {
    if (!input.trim()) return [];
    const res = await getApiPersonsSearch({ query: input });
    const persons: PersonDto[] = res.data ?? [];
    const selectedIds = new Set(value.map((p) => p.id));
    return persons.filter((p) => !selectedIds.has(p.id)).map(mapToOption);
  };

  const handleCreate = (input: string) => {
    const newPerson: PersonDto = { id: null, fullName: input, birthday: null };
    onChange([...value, newPerson]);
  };

  const handleChange = (options: readonly AuthorOption[] | null) => {
    onChange(options ? options.map((o) => o.value) : []);
  };

  return (
    <AsyncCreatableSelect
      isMulti
      cacheOptions
      loadOptions={loadOptions}
      onCreateOption={handleCreate}
      onChange={handleChange}
      value={value.map(mapToOption)}
      getOptionLabel={(o: AuthorOption) => o.label}
      getOptionValue={(o: AuthorOption) => o.value.id ?? o.value.fullName}
      placeholder="Введите имя автора..."
      noOptionsMessage={({ inputValue }) =>
        inputValue ? "Нажмите Enter для добавления нового значения" : null
      }
      formatCreateLabel={(inputValue) => `Add "${inputValue}"`}
      classNames={{
        control: () =>
          "border border-gray-300 rounded-lg px-3 py-2 focus-within:ring-2 focus-within:ring-blue-500",
        multiValue: () => "bg-blue-100 text-blue-800 rounded-full",
        multiValueLabel: () => "text-blue-800 px-2 py-0.5",
        multiValueRemove: () =>
          "text-blue-600 hover:bg-blue-200 rounded-full",
        menu: () => "border border-gray-200 rounded-lg shadow-lg mt-1",
        option: () => "px-3 py-2 hover:bg-blue-50",
        input: () => "text-gray-900",
        placeholder: () => "text-gray-500",
      }}
      styles={{
        control: (base) => ({
          ...base,
          border: "none",
          boxShadow: "none",
          "&:hover": { border: "none" },
        }),
        valueContainer: (base) => ({
          ...base,
          padding: 0,
          gap: "0.25rem",
        }),
        multiValue: (base) => ({
          ...base,
          margin: 0,
        }),
      }}
    />
  );
}
