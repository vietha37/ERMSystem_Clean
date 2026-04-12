import axios from "axios";

export function getApiErrorMessage(
  error: unknown,
  fallback = "Request failed."
): string {
  if (!axios.isAxiosError(error)) {
    return fallback;
  }

  const payload = error.response?.data;

  if (typeof payload === "string" && payload.trim().length > 0) {
    return payload;
  }

  if (payload && typeof payload === "object") {
    const maybeMessage = (payload as { message?: unknown; title?: unknown })
      .message;
    if (typeof maybeMessage === "string" && maybeMessage.trim().length > 0) {
      return maybeMessage;
    }

    const maybeTitle = (payload as { title?: unknown }).title;
    if (typeof maybeTitle === "string" && maybeTitle.trim().length > 0) {
      return maybeTitle;
    }

    const maybeErrors = (payload as { errors?: unknown }).errors;
    if (maybeErrors && typeof maybeErrors === "object") {
      const messages = Object.values(maybeErrors as Record<string, unknown[]>)
        .flat()
        .filter((entry): entry is string => typeof entry === "string");
      if (messages.length > 0) {
        return messages.join(" ");
      }
    }
  }

  return fallback;
}
