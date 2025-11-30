from fastapi import Request, status
from fastapi.responses import JSONResponse
from fastapi.exceptions import RequestValidationError
import traceback
from typing import Union

class ErrorResponse:
    """Standard error response format"""
    
    @staticmethod
    def format_error(
        message: str,
        status_code: int = 400,
        details: dict = None
    ):
        """Format error response"""
        return {
            "erro": message,
            "codigo": status_code,
            "detalhes": details or {}
        }

async def http_exception_handler(request: Request, exc: Exception):
    """Handle HTTP exceptions"""
    status_code = getattr(exc, 'status_code', status.HTTP_500_INTERNAL_SERVER_ERROR)
    message = str(exc)
    
    return JSONResponse(
        status_code=status_code,
        content=ErrorResponse.format_error(message, status_code)
    )

async def validation_exception_handler(request: Request, exc: RequestValidationError):
    """Handle validation errors"""
    errors = []
    for error in exc.errors():
        errors.append({
            "campo": ".".join(str(x) for x in error["loc"][1:]),
            "mensagem": error["msg"]
        })
    
    return JSONResponse(
        status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
        content=ErrorResponse.format_error(
            "Erro de validação nos dados enviados",
            status.HTTP_422_UNPROCESSABLE_ENTITY,
            {"erros": errors}
        )
    )

async def general_exception_handler(request: Request, exc: Exception):
    """Handle general exceptions"""
    traceback.print_exc()
    
    return JSONResponse(
        status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
        content=ErrorResponse.format_error(
            "Erro interno do servidor",
            status.HTTP_500_INTERNAL_SERVER_ERROR
        )
    )
