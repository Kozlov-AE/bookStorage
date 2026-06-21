describe('Vitest', () => {
    it('должен успешно выполнять базовые проверки', () => {
        expect(1 + 1).toBe(2)
        expect('hello').toContain('ell')
        expect([1, 2, 3]).toHaveLength(3)
    })
})