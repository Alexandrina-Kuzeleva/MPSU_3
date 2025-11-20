from dataclasses import dataclass, field
import string


ERR_LENGTH = "length"
ERR_LETTER = "requires_letter"
ERR_DIGIT = "requires_digit"
ERR_SPECIAL = "requires_special"


@dataclass
class PasswordValidationResult:
    is_valid: bool
    errors: list[str] = field(default_factory=list)
    warnings: list[str] = field(default_factory=list)

    def __bool__(self) -> bool:
        return self.is_valid


'''
Требуется проверить минимальную длину пароля (>= 12 символов) и
наличие в пароле хотя бы одной буквы, цифры и спецсимвола.
'''
def validate_password(password: str) -> PasswordValidationResult:
    
    if len(password) < 12:
        return PasswordValidationResult(is_valid=False, errors=ERR_LENGTH)
    
    if not any(char.isalpha() for char in password):
        return PasswordValidationResult(is_valid=False, errors=ERR_LETTER)
        
    if not any(char.isdigit() for char in password):
        return PasswordValidationResult(is_valid=False, errors=ERR_DIGIT)
        
    if not any(char in string.punctuation for char in password):
        return PasswordValidationResult(is_valid=False, errors=ERR_SPECIAL)
    
    return PasswordValidationResult(is_valid=True)