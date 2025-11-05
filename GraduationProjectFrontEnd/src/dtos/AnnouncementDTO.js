export const mapToAnnouncementDTOList = (raw) => {
    if (!Array.isArray(raw)) return [];
    return raw.map(mapOne);
};

export const mapOne = (x) => ({
    id: x.id,
    title: x.title ?? '',
    message: x.message ?? '',
    startAt: x.startAt ? new Date(x.startAt) : null,
    endAt: x.endAt ? new Date(x.endAt) : null,
    isActive: !!x.isActive,
    priority: Number.isFinite(x.priority) ? x.priority : 0,
    lastUpdated: x.lastUpdated ? new Date(x.lastUpdated) : null,
});
