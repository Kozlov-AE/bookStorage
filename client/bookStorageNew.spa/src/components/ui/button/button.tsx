import type {ButtonHTMLAttributes} from "react";
import './button.scss';

type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';
type ButtonSize = 'small' | 'medium' | 'large' | 'icon';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: ButtonVariant;
    size?: ButtonSize;
}

function cn(...classes: (string | false | undefined | null)[]): string {
    return classes.filter(Boolean).join(' ');
}

function Button({variant = 'primary', size = 'medium', className, children, ...props}: ButtonProps) {
    return (
        <button
            className={cn('button', `button--${variant}`, `button--${size}`, className)}
            {...props}
        >
            {children}
        </button>
    )
}

Button.displayName = 'Button';

export {Button};