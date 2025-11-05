import http from './axios';
import { mapToAnnouncementDTOList } from '@/dtos/AnnouncementDTO';

// 取得「目前有效」公告。支援傳入 ETag
export const getActiveAnnouncements = async (prevEtag = null) => {
    const headers = {};
    if (prevEtag) headers['If-None-Match'] = prevEtag;

    try {
        const res = await http.get('/Announcement/active', { headers, validateStatus: () => true });

        if (res.status === 304) {
            return { items: null, etag: prevEtag, notModified: true };
        }
        if (res.status >= 200 && res.status < 300) {
            const etag = res.headers?.etag ?? null;
            return { items: mapToAnnouncementDTOList(res.data), etag, notModified: false };
        }

        throw new Error(`Announcement API ${res.status}`);
    } catch (err) {
        console.error('取得公告失敗：', err);
        throw err;
    }
};

// 若你之後在後台列表要用得到（非必要，先留著）
export const getAllAnnouncements = async (active /* boolean|null */) => {
    const url = active === undefined || active === null
        ? '/Announcement'
        : `/Announcement?active=${active}`;
    const res = await http.get(url);
    return mapToAnnouncementDTOList(res.data);
};
