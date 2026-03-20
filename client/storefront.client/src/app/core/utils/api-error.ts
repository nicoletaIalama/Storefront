import { HttpErrorResponse } from '@angular/common/http';

/** Prefer RFC 7807 `detail`, then legacy `{ message }` from API errors. */
export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof HttpErrorResponse && error.error && typeof error.error === 'object') {
    const body = error.error as { detail?: unknown; message?: unknown };
    if (typeof body.detail === 'string' && body.detail.trim()) {
      return body.detail;
    }
    if (typeof body.message === 'string' && body.message.trim()) {
      return body.message;
    }
  }
  if (error instanceof Error && error.message) {
    return error.message;
  }
  return fallback;
}
