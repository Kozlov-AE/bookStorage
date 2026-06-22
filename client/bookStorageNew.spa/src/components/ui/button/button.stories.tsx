import type { Meta, StoryObj } from '@storybook/react';
import {Button} from './button';

const meta: Meta<typeof Button> = {
    title: 'Ui/Button',
    component: Button,
    argTypes: {
        variant: {
            control: 'select',
            options: ['primary', 'secondary', 'outline', 'ghost', 'danger'],
        },
        size: {
            control: 'select',
            options: ['small', 'medium', 'large'],
        },
        disabled: {
            control: 'boolean'
        },
    },
}

export default meta;

type Story = StoryObj<typeof Button>;

export const Primary: Story = {
    args: {
        variant: 'primary',
        size: 'medium',
        children: 'Button',
    },
};
